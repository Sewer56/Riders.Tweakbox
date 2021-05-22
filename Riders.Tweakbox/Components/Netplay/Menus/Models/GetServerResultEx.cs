using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.API.Application.Commands.v1.User;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Menus.Models
{
    /// <summary>
    /// Extended version of <see cref="GetServerResult"/> for more efficient usage inside server browser.
    /// </summary>
    public class GetServerResultEx : GetServerResult
    {
        public Continent Continent    { get; set; }
        public string ContinentString { get; set; }
        public string CountryString   { get; set; }
        public int? Ping              { get; set; }

        /// <inheritdoc />
        public GetServerResultEx() { }

        /// <inheritdoc />
        public GetServerResultEx(GetServerResult result)
        {
            Mapping.Mapper.From(result).AdaptTo(this);
            Extend();
        }

        /// <summary>
        /// Supplies the values of all calculated properties.
        /// </summary>
        public void Extend()
        {
            var countryAttr = Country.GetCountryAttribute();
            CountryString   = countryAttr.Name;
            Continent       = countryAttr.Continent;
            ContinentString = countryAttr.Continent.GetContinentAttribute().Name;
        }
    }
}
