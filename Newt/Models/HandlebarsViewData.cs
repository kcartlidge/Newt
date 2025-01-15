using System;
using System.Collections.Generic;

namespace Newt.Models;

/// <summary>The data used to feed Handlebars when running a template.</summary>
internal class HandlebarsViewData
{
    public string DataNS { get; set; } = "";
    public string WebNS { get; set; } = "";
    public string Plural { get; set; } = "";
    public string TableName { get; set; } = "";
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string DisplayNamePlural { get; set; } = "";
    public List<DBColumn> Columns { get; set; } = [];
    public List<DBColumn> ReadonlyColumns { get; set; } = [];
    public List<DBColumn> EditableColumns { get; set; } = [];
    public string Comment { get; set; } = "";
    public List<DBTable> Tables { get; set; } = [];
    public Guid RandomGuid { get; set; } = Guid.Empty;
}
