using Flight_Booking.Data;
using Flight_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flight_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CountryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            try
            {
                var countries = await _context.Countries.ToListAsync();
                return Ok(countries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCountries: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching countries", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            try
            {
                var country = await _context.Countries.FindAsync(id);
                if (country == null)
                {
                    return NotFound();
                }
                return country;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCountry: {ex.ToString()}");
                return StatusCode(500, new { message = "Error fetching country", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Country>> CreateCountry(Country country)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateCountry: {ex.ToString()}");
                return StatusCode(500, new { message = "Error creating country", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCountry(int id, Country country)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCountry: {ex.ToString()}");
                return StatusCode(500, new { message = "Error updating country", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                var country = await _context.Countries.FindAsync(id);
                if (country == null)
                {
                    return NotFound();
                }

                var hasRelatedAirlines = await _context.Airline.AnyAsync(a => a.CountryId == id);
                if (hasRelatedAirlines)
                {
                    return BadRequest("It is impossible to delete the country due to the airline associated with this country.");
                }

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCountry: {ex.ToString()}");
                return BadRequest("It is impossible to delete the country due to the relevant data.");
            }
        }
    }
}