using Harrison314.EntityFrameworkCore.Encryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleWebApiProject.Dal;
using SampleWebApiProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PatientController : Controller
    {
        private readonly SampleDbContext context;
        private readonly IDbContextEncryptedProvider<SampleDbContext> encryptionProvider;

        public PatientController(SampleDbContext context, IDbContextEncryptedProvider<SampleDbContext> encryptionProvider)
        {
            this.context = context;
            this.encryptionProvider = encryptionProvider;
        }

        [HttpGet(Name = "GetAllPatients")]
        [ProducesResponseType(typeof(List<PatientInfo>), 200)]
        public async Task<IActionResult> GetAll()
        {
            IEncryptedScopeCreator scopeProvider = await this.encryptionProvider.EnshureEncrypted();
            using IDisposable encryptedScope = scopeProvider.IntoScope();

            List<PatientInfo> result = await this.context.Patients.OrderBy(t => t.Id).Select(t => new PatientInfo()
            {
                FirstName = t.FirstName,
                Id = t.Id,
                LastName = t.LastName
            })
                .ToListAsync();

            return this.Ok(result);
        }

        [HttpGet("{id}", Name = "GetPatient")]
        [ProducesResponseType(typeof(PatientDetail), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id)
        {
            IEncryptedScopeCreator scopeProvider = await this.encryptionProvider.EnshureEncrypted();
            using IDisposable encryptedScope = scopeProvider.IntoScope();

            Patient patient = await this.context.Patients.FindAsync(id);
            if (patient == null)
            {
                return this.NotFound();
            }

            PatientDetail result = new PatientDetail()
            {
                Id = patient.Id,
                Alert = patient.Alert,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Notes = patient.Notes,
                OtherId = patient.OtherId,
                SocialSecurityNumber = patient.SocialSecurityNumber
            };

            return this.Ok(result);
        }

        [HttpGet("/ssn/{ssn}", Name = "GetPatientBySsn")]
        [ProducesResponseType(typeof(PatientDetail), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string ssn)
        {
            IEncryptedScopeCreator scopeProvider = await this.encryptionProvider.EnshureEncrypted();
            using IDisposable encryptedScope = scopeProvider.IntoScope();

            Patient patient = await this.context.Patients.Where(t => t.SocialSecurityNumber == ssn).SingleOrDefaultAsync();
            if (patient == null)
            {
                return this.NotFound();
            }

            PatientDetail result = new PatientDetail()
            {
                Id = patient.Id,
                Alert = patient.Alert,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Notes = patient.Notes,
                OtherId = patient.OtherId,
                SocialSecurityNumber = patient.SocialSecurityNumber
            };

            return this.Ok(result);
        }

        [HttpPut("{id}/Notes", Name = "UpdateNotes")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdatePatientNote model)
        {
            using IDisposable defaultValuesScope = this.encryptionProvider.EnshureDefaultValues().IntoScope();

            Patient patient = await this.context.Patients.FindAsync(id);
            if (patient == null)
            {
                return this.NotFound();
            }

            patient.Notes = model.Notes ?? string.Empty;
            await this.context.SaveChangesAsync();

            return this.Ok();
        }

        [HttpPut("{id}/Name", Name = "UpdateName")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateName(int id, [FromBody] UpdatePatientName model)
        {
            IEncryptedScopeCreator scopeProvider = await this.encryptionProvider.EnshureEncrypted();
            using IDisposable encryptedScope = scopeProvider.IntoScope();

            Patient patient = await this.context.Patients.FindAsync(id);
            if (patient == null)
            {
                return this.NotFound();
            }

            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;

            await this.context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
