#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Newt.ArgsParsing
{
    /// <summary>Handles the assessment of a set of command arguments.</summary>
    internal class Parser
    {
        /// <summary>Were there any errors from parsing?</summary>
        public bool HasErrors { get => ArgumentErrors.Any() || ExpectationErrors.Any(); }

        /// <summary>
        /// Parsing errors regarding the INCOMING arguments.
        /// These are errors in what has been PROVIDED.
        /// These are indexed by their position in the argument set, as this
        /// is the only thing we can be guaranteed to know.
        /// </summary>
        public readonly SortedList<int, string> ArgumentErrors = new SortedList<int, string>();

        /// <summary>
        /// Parsing errors regarding the INCOMING arguments.
        /// These are failures to meet EXPECTATIONS.
        /// They are indexed by the name of the expected argument.
        /// This will never include flags/options as when they are
        /// not provided they default to false.
        /// </summary>
        public readonly SortedList<string, string> ExpectationErrors = new SortedList<string, string>();

        private bool _isParsed = false;
        private readonly string[] _rawArgs;
        private readonly List<string> _options = new List<string>();
        private readonly SortedList<string, object?> _parameters = new SortedList<string, object?>();
        private readonly Dictionary<string, ArgumentDefinition> _knownOptions = new Dictionary<string, ArgumentDefinition>();
        private readonly Dictionary<string, ArgumentDefinition> _knownParameters = new Dictionary<string, ArgumentDefinition>();
        private int MaxParameterWidth => _knownParameters.Max(x => x.Key.Length);
        private int MaxOptionWidth => _knownOptions.Max(x => x.Key.Length);

        /// <summary>Create a parser for the passed-in set of arguments.</summary>
        public Parser(string[] args)
        {
            _rawArgs = args;
        }

        /// <summary>Fluently add an optional named parameter.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="parameterName">Name of the argument.</param>
        /// <param name="info">Description for the end user.</param>
        /// <param name="defaultValue">Any default value if nothing is provided.</param>
        /// <returns>The Parser instance for fluent call chaining.</returns>
        public Parser SupportsParameter<T>(string parameterName, string info, object? defaultValue = null)
        {
            parameterName = parameterName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownParameters.Add(parameterName, new ArgumentDefinition(typeof(T), false, info, defaultValue));
            return this;
        }

        /// <summary>Fluently add a required named parameter.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="parameterName">Name of the argument.</param>
        /// <param name="info">Description for the end user.</param>
        /// <param name="defaultValue">Any default value if nothing is provided.</param>
        /// <returns>The Parser instance for fluent call chaining.</returns>
        public Parser RequiresParameter<T>(string parameterName, string info, object? defaultValue = null)
        {
            parameterName = parameterName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownParameters.Add(parameterName, new ArgumentDefinition(typeof(T), true, info, defaultValue));
            return this;
        }

        /// <summary>Fluently add an optional named flag.</summary>
        /// <param name="optionName">Name of the flag.</param>
        /// <param name="info">Description for the end user.</param>
        /// <returns>The Parser instance for fluent call chaining.</returns>
        public Parser SupportsOption(string optionName, string info)
        {
            optionName = optionName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownOptions.Add(optionName, new ArgumentDefinition(typeof(bool), false, info, null));
            return this;
        }

        /// <summary>Display argument/flag info on the console.</summary>
        public Parser Help()
        {
            // Precalculate space needed for names.
            var req = "(required)";
            var width = Math.Max(MaxParameterWidth, MaxOptionWidth);

            // Display the info for named arguments.
            // List the required ones first.
            var parameterWidth = width;
            if (_knownParameters.Any()) Console.WriteLine();
            foreach (var parameter in _knownParameters
                .OrderByDescending(x => x.Value.IsRequired)
                .ThenBy(y => y.Key))
            {
                var required = parameter.Value.IsRequired ? req : "";
                var key = parameter.Key.PadRight(parameterWidth);
                Console.WriteLine($"-{key} <value>   {parameter.Value.Info} {required}");
            }

            // Display the info for options/flags.
            // List the required ones first, which isn't relevant for
            // options/flags but retained for future consistency.
            var optionWidth = width + (_knownParameters.Any() ? " <value>".Length : 0);
            if (_knownOptions.Any()) Console.WriteLine();
            foreach (var option in _knownOptions
                .OrderByDescending(x => x.Value.IsRequired)
                .ThenBy(y => y.Key))
            {
                var required = option.Value.IsRequired ? req : "";
                var key = option.Key.PadRight(optionWidth);
                Console.WriteLine($"-{key}   {option.Value.Info} {required}");
            }
            Console.WriteLine();
            return this;
        }

        /// <summary>
        /// Attempt to extract relevant info from the set of arguments.
        /// Prior to calling this you should have set up the expected
        /// Parameters and Options.
        /// After calling this you'll have a set of ArgumentErrors and
        /// ExpectationErrors. For checking the actual parsed result
        /// use IsOptionProvided(), IsParameterProvided(), and
        /// GetParameter() to access any values.
        /// </summary>
        public Parser Parse()
        {
            // Can only parse once.
            if (_isParsed) throw new Exception("Parse() can only be called once.");
            _isParsed = true;

            // Get the sequenced arguments.
            var args = new SortedList<int, MatchedValue>();
            var inDash = false;
            for (var i = 0; i < _rawArgs.Length; i++)
            {
                // Assume nothing is known about the current argument.
                var rawArg = _rawArgs[i];
                var arg = new MatchedValue(i + 1, rawArg, EntryKinds.Unknown);

                if (arg.HasDash)
                {
                    // This and the previous are both dashes, so previous must be an option.
                    if (inDash) args[i - 1].KindOfEntry = EntryKinds.Option;

                    // Assume this one will also be an option (parameters patched up below).
                    arg.KindOfEntry = EntryKinds.Option;
                    inDash = true;
                }
                else
                {
                    if (inDash)
                    {
                        // Not a dash, but previous one was, so must be the value for a parameter.
                        args[i - 1].KindOfEntry = EntryKinds.Parameter;
                        args[i - 1].Value = arg.Original;

                        // This one can now be skipped (it became the previous one's value).
                        arg.KindOfEntry = EntryKinds.Skip;
                    }
                    inDash = false;
                }

                // Build up the collection of arguments.
                // Any skipped ones can be discarded.
                if (arg.KindOfEntry != EntryKinds.Skip) args.Add(i, arg);
            }

            // Check for argument errors (things provided but wrong).
            // Populate options and parameters in passing.
            foreach (var arg in args)
            {
                switch (arg.Value.KindOfEntry)
                {
                    case EntryKinds.Unknown:
                        AddArgumentError(arg.Key, $"Unexpected item: {arg.Value.Original}");
                        break;
                    case EntryKinds.Option:
                        if (arg.Value.Name.Length == 0)
                            AddArgumentError(arg.Key, $"Option received with no name");
                        else if (_knownOptions.ContainsKey(arg.Value.Name) == false)
                            AddArgumentError(arg.Key, $"Unknown option: {arg.Value.Name}");
                        else
                            AddOption(arg.Value.Name);
                        break;
                    case EntryKinds.Parameter:
                        if (arg.Value.Name.Length == 0)
                            AddArgumentError(arg.Key, $"Parameter received with no name");
                        else if (_knownParameters.ContainsKey(arg.Value.Name) == false)
                            AddArgumentError(arg.Key, $"Unknown parameter: {arg.Value.Name}");
                        else
                        {
                            var p = _knownParameters[arg.Value.Name];
                            AddParameter(arg.Value.Name);
                            try
                            {
                                _parameters[arg.Value.Name] = Convert.ChangeType(arg.Value.Value, p.DotNetType);
                            }
                            catch
                            {
                                AddArgumentError(arg.Value.Sequence, $"Expected a value of type {p.DotNetType}: {arg.Value.Name}");
                            }
                        }
                        break;
                    case EntryKinds.Skip:
                    default:
                        break;
                }
            }

            // Check for required arguments having been provided.
            foreach (var parameter in _knownParameters.Where(x => x.Value.IsRequired))
                if (_parameters.ContainsKey(parameter.Key) == false)
                    // Apply a default if provided, else it's an error.
                    if (parameter.Value.DefaultValue != null)
                        _parameters.Add(parameter.Key, parameter.Value.DefaultValue);
                    else
                        AddExpectationError(parameter.Key, $"Parameter missing: {parameter.Key}");

            return this;
        }

        /// <summary>Has the option/flag been provided?</summary>
        public bool IsOptionProvided(string optionName)
        {
            optionName = optionName.ToLowerInvariant().Trim();
            if (_isParsed == false)
                throw new Exception($"Cannot access '{optionName}' before parsing.");

            if (_knownOptions.ContainsKey(optionName) == false)
                throw new ArgumentException($"Unknown option: {optionName}");

            return (_options.Contains(optionName));
        }

        /// <summary>Has the argument been provided?</summary>
        public bool IsParameterProvided(string parameterName)
        {
            parameterName = parameterName.ToLowerInvariant().Trim();
            if (_isParsed == false)
                throw new Exception($"Cannot access '{parameterName}' before parsing.");

            if (_knownParameters.ContainsKey(parameterName) == false)
                throw new ArgumentException($"Unknown parameter: {parameterName}");

            return (_parameters.ContainsKey(parameterName));
        }

        /// <summary>
        /// Get the provided argument value, cast to the required type
        /// (which must match the type given when it was set up).
        /// </summary>
        public T GetParameter<T>(string parameterName)
        {
            parameterName = parameterName.ToLowerInvariant().Trim();
            if (_isParsed == false)
                throw new Exception($"Cannot access '{parameterName}' before parsing.");

            // Unknown parameter.
            if (_knownParameters.ContainsKey(parameterName) == false)
                throw new ArgumentException($"Unknown parameter: {parameterName}");

            // Parameter type and generic type on this call differ.
            if (typeof(T) != _knownParameters[parameterName].DotNetType)
                throw new InvalidCastException($"Incorrect type getting parameter {parameterName}");

            // Parameter value was provided (default is already applied if needed).
            if (IsParameterProvided(parameterName))
                return (T)_parameters[parameterName];

            // No parameter provided, but there is a default given.
            if (_knownParameters[parameterName].DefaultValue != null)
                return (T)_knownParameters[parameterName].DefaultValue;

                // Fallback on the default value for the return type.
            return default;
        }

        /* SUPPORT */

        /// <summary>
        /// Record an error with a sequenced input argument.
        /// Multiples are registered with line-break delimiters.
        /// </summary>
        private void AddArgumentError(int sequence, string message)
        {
            if (ArgumentErrors.ContainsKey(sequence))
                ArgumentErrors[sequence] = ArgumentErrors[sequence] + $"\n{message}";
            else
                ArgumentErrors.Add(sequence, message);
        }

        /// <summary>
        /// Record an error regarding an expected parameter/option.
        /// Multiples are registered with line-break delimiters.
        /// </summary>
        private void AddExpectationError(string key, string message)
        {
            if (ExpectationErrors.ContainsKey(key))
                ExpectationErrors[key] = ExpectationErrors[key] + $"\n{message}";
            else
                ExpectationErrors.Add(key, message);
        }

        /// <summary>Add the named option.</summary>
        private void AddOption(string optionName)
        {
            if (_options.Contains(optionName)) return;
            _options.Add(optionName);
        }

        /// <summary>Add the named parameter (without a value).</summary>
        private void AddParameter(string parameterName)
        {
            if (_parameters.ContainsKey(parameterName)) return;
            _parameters.Add(parameterName, null);
        }
    }
}