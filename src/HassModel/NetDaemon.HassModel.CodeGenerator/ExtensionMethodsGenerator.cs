using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class ExtensionMethodsGenerator
{
    public static IEnumerable<MemberDeclarationSyntax> Generate(IEnumerable<HassServiceDomain> serviceDomains, IReadOnlyCollection<HassState> entities)
    {
        var entityDomains = entities.GroupBy(e => EntityIdHelper.GetDomain(e.EntityId)).Select(x => x.Key);

        return serviceDomains
            .Where(sd =>
                sd.Services?.Any() == true
                && sd.Services.Any(s => entityDomains.Contains(s.Target?.Entity?.Domain)))
            .GroupBy(x => x.Domain, x => x.Services)
            .Select(GenerateClass);
    }

    private static ClassDeclarationSyntax GenerateClass(IGrouping<string?, IReadOnlyCollection<HassService>?> domainServicesGroup)
    {
        var domain = domainServicesGroup.Key!;

        var domainServices = domainServicesGroup
            .SelectMany(services => services!)
            .Where(s => s.Target?.Entity?.Domain != null)
            .Select(group => @group)
            .OrderBy(x => x.Service)
            .ToList();

        return GenerateDomainExtensionClass(domain, domainServices);
    }

    private static ClassDeclarationSyntax GenerateDomainExtensionClass(string domain, IEnumerable<HassService> services)
    {
        var serviceTypeDeclaration = Class(GetEntityDomainExtensionMethodClassName(domain)).ToPublic().ToStatic();

        var serviceMethodDeclarations = services
            .SelectMany(service => GenerateExtensionMethod(domain, service))
            .ToArray();

        return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateExtensionMethod(string domain, HassService service)
    {
        var serviceName = service.Service!;

        var serviceArguments = ServiceArguments.Create(domain, service);

        var entityTypeName = GetDomainEntityTypeName(service.Target?.Entity?.Domain!);

        yield return ParseMethod(
                $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} entity {(serviceArguments is not null ? $", {serviceArguments.GetParametersString()}" : string.Empty)})
            {{
                entity.CallService(""{serviceName}""{(serviceArguments is not null ? $", {serviceArguments.GetParametersVariable()}" : string.Empty)});
            }}").ToPublic().ToStatic()
            .WithSummaryComment(service.Description);

        if (serviceArguments is not null)
        {
            yield return ParseMethod(
                    $@"void {GetServiceMethodName(serviceName)}(this {entityTypeName} entity, {serviceArguments.GetParametersDecomposedString()})
                {{
                    entity.CallService(""{serviceName}"", {serviceArguments.GetParametersDecomposedVariable()});
                }}").ToPublic().ToStatic()
                .WithSummaryComment(service.Description)
                .WithParameterComment("entity", $"The {entityTypeName} to call this service for")
                .WithParameterComments(serviceArguments);
        }
    }
}
