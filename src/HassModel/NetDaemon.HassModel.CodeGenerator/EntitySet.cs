﻿using System.Collections.Generic;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator.Extensions;
using NetDaemon.HassModel.CodeGenerator.Helpers;

namespace NetDaemon.HassModel.CodeGenerator
{
    internal record EntitySet(string Domain, bool IsNumeric, IEnumerable<HassState> EntityStates)
    {
        private readonly string prefixedDomain = (IsNumeric && Domain != "input_number" ? "numeric_" : "") + Domain;

        public string EntityClassName => NamingHelper.GetDomainEntityTypeName(prefixedDomain);

        public string AttributesClassName => $"{prefixedDomain}Attributes".ToNormalizedPascalCase();

        public string EntitiesForDomainClassName => $"{Domain}Entities".ToNormalizedPascalCase();
    }
}