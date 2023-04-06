global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.Json;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using NetDaemon.HassModel.CodeGenerator;
global using NetDaemon.HassModel.CodeGenerator.Model;
global using NetDaemon.HassModel.CodeGenerator.Helpers;
global using NetDaemon.HassModel.CodeGenerator.Extensions;
global using NetDaemon.HassModel.Entities;
global using Microsoft.CodeAnalysis;
global using static NetDaemon.HassModel.CodeGenerator.Helpers.NamingHelper;
global using static NetDaemon.HassModel.CodeGenerator.Helpers.SyntaxFactoryHelper;

// This is needed to allow integration tests to run code generation and parsing without major refactoring
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("NetDaemon.Tests.Integration")]
