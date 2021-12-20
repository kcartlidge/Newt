using System;
using System.Collections.Generic;
using System.Linq;

namespace Newt.ArgsParsing
{
    public class Parser
    {
        public bool HasErrors { get => ArgumentErrors.Any() || ExpectationErrors.Any(); }
        public readonly SortedList<int, string> ArgumentErrors = new SortedList<int, string>();
        public readonly SortedList<string, string> ExpectationErrors = new SortedList<string, string>();

        private bool _parsed = false;
        private readonly string[] _rawArgs;
        private readonly List<string> _options = new List<string>();
        private readonly SortedList<string, object?> _parameters = new SortedList<string, object?>();
        private readonly Dictionary<string, ArgDetail> _knownOptions = new Dictionary<string, ArgDetail>();
        private readonly Dictionary<string, ArgDetail> _knownParameters = new Dictionary<string, ArgDetail>();
        private int MaxParameterWidth => _knownParameters.Max(x => x.Key.Length);
        private int MaxOptionWidth => _knownOptions.Max(x => x.Key.Length);

        public Parser(string[] args)
        {
            _rawArgs = args;
        }

        public Parser SupportsParameter<T>(string parameterName, string info, object? defaultValue = null)
        {
            parameterName = parameterName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownParameters.Add(parameterName, new ArgDetail(typeof(T), false, info, defaultValue));
            return this;
        }

        public Parser RequiresParameter<T>(string parameterName, string info, object? defaultValue = null)
        {
            parameterName = parameterName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownParameters.Add(parameterName, new ArgDetail(typeof(T), true, info, defaultValue));
            return this;
        }

        public Parser SupportsOption(string optionName, string info)
        {
            optionName = optionName.ToLowerInvariant().Trim().TrimStart('-').Trim();

            _knownOptions.Add(optionName, new ArgDetail(typeof(bool), false, info, null));
            return this;
        }

        public Parser Help()
        {
            var req = "(required)";
            var width = Math.Max(MaxParameterWidth, MaxOptionWidth);
            var parameterWidth = width;
            if (_knownParameters.Any()) Console.WriteLine();
            foreach (var parameter in _knownParameters.OrderByDescending(x => x.Value.IsRequired).ThenBy(y => y.Key))
            {
                var required = parameter.Value.IsRequired ? req : "";
                var key = parameter.Key.PadRight(parameterWidth);
                Console.WriteLine($"-{key} <value>   {parameter.Value.Info} {required}");
            }
            var optionWidth = width + (_knownParameters.Any() ? " <value>".Length : 0);
            if (_knownOptions.Any()) Console.WriteLine();
            foreach (var option in _knownOptions.OrderByDescending(x => x.Value.IsRequired).ThenBy(y => y.Key))
            {
                var required = option.Value.IsRequired ? req : "";
                var key = option.Key.PadRight(optionWidth);
                Console.WriteLine($"-{key}   {option.Value.Info} {required}");
            }
            Console.WriteLine();
            return this;
        }

        public Parser Parse()
        {
            // Can only parse once.
            if (_parsed) return this;
            _parsed = true;

            // Get the sequenced arguments.
            var args = new SortedList<int, ArgValue>();
            var inDash = false;
            for (var i = 0; i < _rawArgs.Length; i++)
            {
                // Assume nothing is known about the current argument.
                var rawArg = _rawArgs[i];
                var arg = new ArgValue(i + 1, rawArg, ArgType.Unknown);

                if (arg.HasDash)
                {
                    // This and previous both dashes, so previous must be an option.
                    if (inDash)
                        args[i - 1].ArgType = ArgType.Option;

                    // Assume this will just be an option (parameters are patched up below).
                    arg.ArgType = ArgType.Option;
                    inDash = true;
                }
                else
                {
                    if (inDash)
                    {
                        // Not a dash, but previous one was, so must be the value for a parameter.
                        args[i - 1].ArgType = ArgType.Parameter;
                        args[i - 1].Value = arg.Original;
                        arg.ArgType = ArgType.Skip;
                    }
                    inDash = false;
                }

                // Build up the collection of arguments.
                if (arg.ArgType != ArgType.Skip) args.Add(i, arg);
            }

            // Check for argument errors (things provided but wrong).
            // Populate options and parameters in passing.
            foreach (var arg in args)
            {
                switch (arg.Value.ArgType)
                {
                    case ArgType.Unknown:
                        AddArgumentError(arg.Key, $"Unexpected item: {arg.Value.Original}");
                        break;
                    case ArgType.Option:
                        if (arg.Value.Name.Length == 0)
                            AddArgumentError(arg.Key, $"Option received with no name");
                        else if (_knownOptions.ContainsKey(arg.Value.Name) == false)
                            AddArgumentError(arg.Key, $"Unknown option: {arg.Value.Name}");
                        else
                            AddOption(arg.Value.Name);
                        break;
                    case ArgType.Parameter:
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
                                _parameters[arg.Value.Name] = Convert.ChangeType(arg.Value.Value, p.ArgType);
                            }
                            catch
                            {
                                AddArgumentError(arg.Value.Sequence, $"Expected a value of type {p.ArgType}: {arg.Value.Name}");
                            }
                        }
                        break;
                    case ArgType.Skip:
                    default:
                        break;
                }
            }
            
            foreach (var parameter in _knownParameters.Where(x => x.Value.IsRequired))
                if (_parameters.ContainsKey(parameter.Key) == false)
                    // Apply a default if provided, else it's an error.
                    if (parameter.Value.DefaultValue != null)
                        _parameters.Add(parameter.Key, parameter.Value.DefaultValue);
                    else
                        AddExpectationError(parameter.Key, $"Parameter missing: {parameter.Key}");

            return this;
        }

        public bool IsOptionProvided(string optionName)
        {
            optionName = optionName.ToLowerInvariant().Trim();

            if (_knownOptions.ContainsKey(optionName) == false)
                throw new ArgumentException($"Unknown option: {optionName}");

            return (_options.Contains(optionName));
        }

        public bool IsParameterProvided(string parameterName)
        {
            parameterName = parameterName.ToLowerInvariant().Trim();

            if (_knownParameters.ContainsKey(parameterName) == false)
                throw new ArgumentException($"Unknown parameter: {parameterName}");

            return (_parameters.ContainsKey(parameterName));
        }
        
        public T GetParameter<T>(string parameterName)
        {
            parameterName = parameterName.ToLowerInvariant().Trim();

            // Unknown parameter.
            if (_knownParameters.ContainsKey(parameterName) == false)
                throw new ArgumentException($"Unknown parameter: {parameterName}");

            // Parameter type and generic type on this call differ.
            if (typeof(T) != _knownParameters[parameterName].ArgType)
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

        private void AddArgumentError(int sequence, string message)
        {
            if (ArgumentErrors.ContainsKey(sequence))
                ArgumentErrors[sequence] = ArgumentErrors[sequence] + $"\n{message}";
            else
                ArgumentErrors.Add(sequence, message);
        }

        private void AddExpectationError(string key, string message)
        {
            if (ExpectationErrors.ContainsKey(key))
                ExpectationErrors[key] = ExpectationErrors[key] + $"\n{message}";
            else
                ExpectationErrors.Add(key, message);
        }

        private void AddOption(string optionName)
        {
            if (_options.Contains(optionName)) return;
            _options.Add(optionName);
        }

        private void AddParameter(string parameterName)
        {
            if (_parameters.ContainsKey(parameterName)) return;
            _parameters.Add(parameterName, null);
        }
    }
}