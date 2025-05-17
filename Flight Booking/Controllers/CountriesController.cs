using Flight_Booking.Data;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CountriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            return await _context.Countries.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return country;
        }

        [HttpPost]
        public async Task<ActionResult<Country>> CreateCountry(Country country)
        {
            var codeExists = await _context.Countries.AnyAsync(c => c.Code.ToUpper() == country.Code.ToUpper());
            if (codeExists)
            {
                return BadRequest("The country code has existed.");
            }

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(int id, Country country)
        {
            country.Id = id;

            var existingCountry = await _context.Countries.FindAsync(id);
            if (existingCountry == null)
            {
                return NotFound();
            }

            var codeExists = await _context.Countries.AnyAsync(c => c.Code.ToUpper() == country.Code.ToUpper() && c.Id != id);
            if (codeExists)
            {
                return BadRequest("The country code has existed.");
            }

            _context.Entry(existingCountry).CurrentValues.SetValues(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            // Kiểm tra các hãng hàng không liên quan
            var hasRelatedAirlines = await _context.Airlines.AnyAsync(a => a.CountryId == id);
            if (hasRelatedAirlines)
            {
                return BadRequest("It is impossible to delete the country due to the airline associated with this country.");
            }

            try
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return BadRequest("It is impossible to delete the country due to the relevant data.");
            }
        }
    }
}