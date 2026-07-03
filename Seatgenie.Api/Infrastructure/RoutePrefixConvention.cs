using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Seatgenie.Api.Infrastructure;

/// <summary>
/// Prepends a global path prefix (e.g. "api") to every attribute-routed action,
/// so controllers can keep declaring clean absolute routes like "/offices".
/// </summary>
public class RoutePrefixConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel _prefix;

    public RoutePrefixConvention(string prefix)
        => _prefix = new AttributeRouteModel(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel is null
                    ? _prefix
                    : AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
            }

            foreach (var action in controller.Actions)
            {
                foreach (var selector in action.Selectors)
                {
                    if (selector.AttributeRouteModel is not null)
                    {
                        selector.AttributeRouteModel =
                            AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
                    }
                }
            }
        }
    }
}
