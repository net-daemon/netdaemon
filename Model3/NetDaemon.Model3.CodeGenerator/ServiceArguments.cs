using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Service.App.CodeGeneration.Extensions;
using NetDaemon.Service.App.CodeGeneration.Helpers;
namespace NetDaemon.Service.App.CodeGeneration
{
    internal record ServiceArgument
    {
        public Type? Type { get; init; }

        public bool Required { get; init; }

        public string? HaName { get; init; }

        public string? TypeName => Type?.GetFriendlyName();

        public string? ParameterTypeName => Required ? TypeName : $"{TypeName}?";

        public string? PropertyName => HaName?.ToNormalizedPascalCase();

        public string? VariableName => HaName?.ToNormalizedCamelCase();

        public string? ParameterVariableName => Required ? VariableName : $"{VariableName} = null";
    }

    internal class ServiceArguments
    {
        private readonly string _serviceName;
        private readonly string _domain;

        public ServiceArguments(string domain, string serviceName, IReadOnlyCollection<HassServiceField> serviceFields)
        {
            _domain = domain;
            _serviceName = serviceName!;
            Arguments = serviceFields.Select(HassServiceArgumentMapper.Map);
        }

        public IEnumerable<ServiceArgument> Arguments { get; }

        public bool HasRequiredArguments => Arguments.Any(v => v.Required);

        public string TypeName => NamingHelper.GetServiceArgumentsTypeName(_domain, _serviceName);

        public string GetParametersString()
        {
            // adding {(HasRequiredArguments ? "" : "= null")} causes ambiguity in a call with optional args
            return $"{TypeName} data";
        }

        [SuppressMessage("", "CA1822", Justification = "we might want to use specific arg name in the future")]
        public string GetParametersVariable()
        {
            return "data";
        }

        public string GetParametersDecomposedString()
        {
            var argumentList = Arguments.OrderByDescending(arg => arg.Required);

            var anonymousVariableStr = argumentList.Select(x => $"{x.ParameterTypeName} @{x.ParameterVariableName}");

            return $"{string.Join(", ", anonymousVariableStr)}";
        }

        public string GetParametersDecomposedVariable()
        {
            var anonymousVariableStr = Arguments.Select(x => $"@{x.HaName} = @{x.VariableName}");

            return $"new {{ {string.Join(", ", anonymousVariableStr)} }}";
        }
    }
}