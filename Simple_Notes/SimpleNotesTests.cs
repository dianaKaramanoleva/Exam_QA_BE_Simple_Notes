using RestSharp;
using RestSharp.Authenticators;
using Simple_Notes_Tests.DTOs;
using System;
using System.Net;
using System.Text.Json;


namespace Simple_Notes
{
    public class SimpleNotes_Tests
    {
        private RestClient client;
        private static string noteId;
        string baseURL = "http://144.91.123.158:5005";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("diana_K@abv.bg", "diana123");
            RestClientOptions options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)

            };
            client = new RestClient(options);
        }
                
        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient(baseURL);
            RestRequest request = new RestRequest("/api/User/Authorization", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = authClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }

            else
            {
                throw new InvalidOperationException($"Failed to authorize. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }
        [Order(1)]
        [Test]
        public void CreateNote_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            NoteDto note = new NoteDto();

            RestRequest request = new RestRequest("/api/Note/Create", Method.Post);

            // missing Title, Description, Status
            request.AddJsonBody(note);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest),
                $"Expected 400 BadRequest but got {(int)response.StatusCode}. Response: {response.Content}");
        }

        [Order(2)]
        [Test]
        public void CreateNote_WithRequiredFields_ShouldSuccess()
        {
            NoteDto note = new NoteDto
            {
                Title = "My first note",
                Description = "This is very important note! Pay attention to it!",
                Status = "New",
                //StartDate = "2026-04-23",
                //EndDate = "2026-04-24"
            };

            RestRequest request = new RestRequest("/api/Note/Create", Method.Post);
            request.AddJsonBody(note);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Note created successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllNotes_ShouldReturnNotEmptyList()
        {
            RestRequest request = new RestRequest("/api/Note/AllNotes", Method.Get);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var notes = JsonSerializer.Deserialize<List<NoteDto>>(
               JsonDocument.Parse(response.Content).RootElement.GetProperty("allNotes").GetRawText()
            );

            Assert.That(notes, Is.Not.Null);
            Assert.That(notes.Count, Is.GreaterThan(0));

            NoteDto lastNote = notes.Last();

            noteId = lastNote.Id;

            Assert.That(noteId, Is.Not.Null.And.Not.Empty);
        }

        [Order(4)]
        [Test]
        public void EditNote_WithValidData_ShouldSuccess()
        {
            Assert.That(noteId, Is.Not.Null.And.Not.Empty, "CreatedNoteId is missing.");

            NoteDto editedNote = new NoteDto
            {
                Title = "This is better note",
                Description = "This is very important note! Pay attention to it!",
                Status = "Done",
                //StartDate = "2026-04-23",
                //EndDate = "2026-04-24"
            };

            RestRequest request = new RestRequest($"/api/Note/Edit/{noteId}", Method.Put);
            request.AddJsonBody(editedNote);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Note edited successfully!"));
        }

        [Order(5)]
        [Test]
        public void DeleteExistingNote_ShouldSuccess()
        {

            Assert.That(noteId, Is.Not.Null.And.Not.Empty, "CreatedNoteId is missing.");

            RestRequest request = new RestRequest($"/api/Note/Delete/{noteId}", Method.Delete);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Note deleted successfully!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }

}