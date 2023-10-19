using System.Globalization;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

/*
    Attribution to the devs at https://github.com/andrewlock/NetEscapades.Configuration for providing most of the code
    for the file parser. Thanks a lot!
*/

namespace NetDaemon.AppModel.Internal.Config;

internal class YamlConfigurationFileParser
{
    private readonly Stack<string> _context = new();

    private readonly IDictionary<string, string?> _data =
        new SortedDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    private string _currentPath = string.Empty;

    public IDictionary<string, string?> Parse(Stream input)
    {
        _data.Clear();
        _context.Clear();

        var yaml = new YamlStream();
        yaml.Load(new StreamReader(input, true));

        if (yaml.Documents.Count == 0) return _data;
        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

        // The document node is a mapping node
        VisitYamlMappingNode(mapping);

        return _data;
    }

    private void VisitYamlNodePair(KeyValuePair<YamlNode, YamlNode> yamlNodePair)
    {
        var (key, value) = yamlNodePair;
        var context = ((YamlScalarNode)key).Value ?? string.Empty;
        VisitYamlNode(context, value);
    }

    private void VisitYamlNode(string context, YamlNode node)
    {
        switch (node)
        {
            case YamlScalarNode scalarNode:
                VisitYamlScalarNode(context, scalarNode);
                break;
            case YamlMappingNode mappingNode:
                VisitYamlMappingNode(context, mappingNode);
                break;
            case YamlSequenceNode sequenceNode:
                VisitYamlSequenceNode(context, sequenceNode);
                break;
        }
    }

    private void VisitYamlScalarNode(string context, YamlScalarNode yamlValue)
    {
        //a node with a single 1-1 mapping
        EnterContext(context);
        var currentKey = _currentPath;

        _data[currentKey] = IsNullValue(yamlValue) ? null : yamlValue.Value;
        ExitContext();
    }

    private void VisitYamlMappingNode(YamlMappingNode node)
    {
        foreach (var yamlNodePair in node.Children) VisitYamlNodePair(yamlNodePair);
    }

    private void VisitYamlMappingNode(string context, YamlMappingNode yamlValue)
    {
        //a node with an associated sub-document
        EnterContext(context);

        VisitYamlMappingNode(yamlValue);

        ExitContext();
    }

    private void VisitYamlSequenceNode(string context, YamlSequenceNode yamlValue)
    {
        //a node with an associated list
        EnterContext(context);

        VisitYamlSequenceNode(yamlValue);

        ExitContext();
    }

    private void VisitYamlSequenceNode(YamlSequenceNode node)
    {
        for (var i = 0; i < node.Children.Count; i++) VisitYamlNode(i.ToString(CultureInfo.InvariantCulture), node.Children[i]);
    }

    private void EnterContext(string context)
    {
        _context.Push(context);
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        _context.Pop();
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private static bool IsNullValue(YamlScalarNode yamlValue)
    {
        return yamlValue.Style == ScalarStyle.Plain
               && yamlValue.Value is "~" or "null" or "Null" or "NULL";
    }
}
