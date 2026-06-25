using Microsoft.AspNetCore.Mvc;
using PharmacyApi.Models;
using PharmacyApi.Services;

namespace PharmacyApi.Controllers
{
    /// <summary>
    /// Controller for managing medicines in the pharmacy API.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        /// <summary>
        /// The service that handles medicine-related operations
        /// </summary>
        private readonly IMedicineService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicinesController"/> class with the specified medicine service.
        /// </summary>
        /// <param name="service"><!-- Medicine service --></param>
        public MedicinesController(IMedicineService service) => _service = service;

        /// <summary>
        /// Get all medicines or search by name
        /// </summary>
        /// <param name="search"><!-- Search term --></param>
        /// <returns><!-- List of medicines --></returns>
        [HttpGet]
        public ActionResult<IEnumerable<Medicine>> GetAll([FromQuery] string? search)
        {
            var result = string.IsNullOrWhiteSpace(search)
                ? _service.GetAll()
                : _service.Search(search);
            return Ok(result);
        }

        /// <summary>
        /// Get a medicine by its ID
        /// </summary>
        /// <param name="id"><!-- Medicine ID --></param>
        /// <returns><!-- Medicine --></returns>

        [HttpGet("{id:int}")]
        public ActionResult<Medicine> GetById(int id)
        {
            var medicine = _service.GetById(id);
            return medicine is null ? NotFound() : Ok(medicine);
        }

        /// <summary>
        /// Create a new medicine
        /// </summary>
        /// <param name="request"><!-- Medicine request --></param>
        /// <returns><!-- Created medicine --></returns>
        [HttpPost]
        public ActionResult<Medicine> Create([FromBody] CreateMedicineRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = _service.Add(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}
