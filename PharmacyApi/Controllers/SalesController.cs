using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    /// <summary>
    /// Controller for managing sales in the pharmacy API.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        /// <summary>
        /// The service that handles sale-related operations
        /// </summary>
        private readonly ISaleService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesController"/> class with the specified sale service.
        /// </summary>
        /// <param name="service"><!-- Sale service --></param>
        public SalesController(ISaleService service) => _service = service;

        /// <summary>
        /// Gets all sales records.
        /// </summary>
        /// <returns><!-- List of sales --></returns>
        [HttpGet]
        public ActionResult<IEnumerable<Sale>> GetAll() => Ok(_service.GetAll());

        /// <summary>
        /// Creates a new sale record based on the provided request data.
        /// </summary>
        /// <param name="request"><!-- Sale request --></param>
        /// <returns><!-- Created sale --></returns>
        [HttpPost]
        public ActionResult<Sale> Create([FromBody] CreateSaleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var sale = _service.RecordSale(request);
                return CreatedAtAction(nameof(GetAll), new { id = sale.Id }, sale);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
