using Microsoft.AspNetCore.Mvc;
using SampleWebApiProject.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Bogus.Extensions.UnitedStates;
using Bogus.Extensions.Norway;
using Bogus.DataSets;
using Harrison314.EntityFrameworkCore.Encryption;

namespace SampleWebApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelperController : Controller
    {
        private readonly SampleDbContext context;
        private readonly IDbContextEncryptedProvider<SampleDbContext> encryptionProvider;

        public HelperController(SampleDbContext context, IDbContextEncryptedProvider<SampleDbContext> encryptionProvider)
        {
            this.context = context;
            this.encryptionProvider = encryptionProvider;
        }

        [HttpPost(Name = "CreateData")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateData()
        {

            await this.context.Database.EnsureCreatedAsync();

            IEncryptedScopeCreator scopeProvider = await this.encryptionProvider.EnshureEncrypted();
            using IDisposable encryptedScope = scopeProvider.IntoScope();

            Faker<Patient> patientFaker = new Faker<Patient>()
                .RuleFor(t => t.Alert, t => t.Random.Bool())
                .RuleFor(t => t.FirstName, t => t.Name.FirstName())
                .RuleFor(t => t.LastName, t => t.Name.LastName())
                .RuleFor(t => t.Notes, t => t.Lorem.Sentences(t.Random.Number(0, 3)))
                .RuleFor(t => t.SocialSecurityNumber, t => t.Person.Ssn())
                .RuleFor(t => t.OtherId, t => t.Person.Fødselsnummer());

            Faker<Visist> visitFaker = new Faker<Visist>()
                .RuleFor(t => t.Braces, t => t.Random.Number(0, 3))
                .RuleFor(t => t.DentalPlaqueState, t => t.Random.Number(0, 7))
                .RuleFor(t => t.FixSupport, t => t.Random.Number(0, 3))
                .RuleFor(t => t.FurkationResoprtion, t => t.Random.Number(0, 3))
                .RuleFor(t => t.GingivalBleedingState, t => t.Random.Number(0, 3))
                .RuleFor(t => t.GingivalCamsState, t => t.Random.Number(0, 3))
                .RuleFor(t => t.GingivalCamsTherapy, t => t.Random.Number(0, 3))
                .RuleFor(t => t.GumsState, t => t.Random.Number(0, 7))
                .RuleFor(t => t.Pigment, t => t.Random.Number(0, 3))
                .RuleFor(t => t.TartarState, t => t.Random.Number(0, 3))
                .RuleFor(t => t.Date, t => t.Date.Between(DateTime.Now.AddDays(-365), DateTime.Now.AddDays(-5)))
                .RuleFor(t => t.Note, t => t.Lorem.Sentences(t.Random.Number(0, 15)))
                .RuleFor(t => t.Price, t => t.Random.ListItem(new List<double>() { 25.0, 30.0, 50.0, 45.0 }));

            for (int i = 0; i < 42; i++)
            {
                Patient patient = patientFaker.Generate();
                patient.Visists = new HashSet<Visist>();

                for (int j = 0; j < 7; j++)
                {
                    Visist visit = visitFaker.Generate();
                    patient.Visists.Add(visit);
                }

                await this.context.Patients.AddAsync(patient);
            }

            await this.context.SaveChangesAsync();

            return this.Ok();
        }
    }
}
