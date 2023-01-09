using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using CarnetDeContact.Models;

namespace CarnetDeContact.Repo
{
    internal class APIClient
    {
        private List<Contact> _contacts;
        private HttpClient _httpClient;

        public APIClient()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "xxxx");

            _httpClient.BaseAddress = new Uri("https://localhost:5000/api/v1/");
        }

        public async Task<List<Contact>> GetContactsAsync()
        {
            if (_contacts == null)
            {
                //var response = await _httpClient.GetAsync("contacts/");
                //_contacts = await JsonSerializer.DeserializeAsync<List<Contact>>(response.Content.ReadAsStream());
                //response.EnsureSuccessStatusCode();

                _contacts = new List<Contact> {
                    new Contact(){ Id = 1, Name = "Robert", FirstName = "Loïc", Email = "loic.rob@gmail.com", PhoneNumber = "0685598736"},
                    new Contact(){ Id = 2, Name = "Barbier", FirstName = "Jérôme", Email = "jerome.barbier@viacesi.fr", PhoneNumber = "0836656565"},
                    new Contact(){ Id = 3, Name = "Monnier", FirstName = "Titouan", Email = "titouan.monnier@viacesi.fr", PhoneNumber = "0559401939"},
                };
            }
            _contacts.ForEach(p => p.SetInitialLoadValue());
            return _contacts.OrderBy(p => p.Name).ToList();
        }

        public async Task<bool> DeleteContactAsync(Contact contact)
        {
            //var response = await _httpClient.DeleteAsync($"contacts/{prod.Id}");
            //response.EnsureSuccessStatusCode();

            _contacts.Remove(contact);
            return true;
        }

        public async Task<bool> AddContactAsync(Contact contact)
        {
            //var body = new StringContent(JsonSerializer.Serialize(prod), Encoding.UTF8, "application/json");
            //var response = await _httpClient.PostAsync($"contacts/{prod.Id}", body);
            //response.EnsureSuccessStatusCode();

            _contacts.Add(new Contact
            {
                Id = _contacts.Max(p => p.Id) + 1,
                Name = contact.Name,
                FirstName = contact.FirstName,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber
            });

            return true;
        }

        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            //var body = new StringContent(JsonSerializer.Serialize(prod), Encoding.UTF8, "application/json");
            //var response = await _httpClient.PatchAsync($"contacts/{prod.Id}", body);
            //response.EnsureSuccessStatusCode();

            contact.SetPendingChanges(false);
            return true;
        }
    }
}
