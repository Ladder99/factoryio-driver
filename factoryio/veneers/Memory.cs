using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.factoryio.veneers
{
    public class Memory : Veneer
    {
        public Memory(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                type = string.Empty,
                name = string.Empty,
                address = string.Empty,
                kind = string.Empty,
                value = string.Empty
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            var current_value = new
            {
                input.type,
                input.name,
                input.address,
                input.kind,
                input.value
            };
            
            await onDataArrivedAsync(input, current_value);
            
            if (current_value.IsDifferentString((object)lastChangedValue))
            {
                await onDataChangedAsync(input, current_value);
            }
            
            return new { veneer = this };
        }
    }
}