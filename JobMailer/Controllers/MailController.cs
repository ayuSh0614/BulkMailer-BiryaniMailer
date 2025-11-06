using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JobMailer.Controllers
{
    
    public class MailController : Controller
    {

        

        public IActionResult Index()
        {
            return View("MainView");
        }

        
        [HttpPost]
        public async Task<IActionResult> SendMails(IFormFile file, string subject, string content)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please upload a CSV file.";
                return View("MainView");
            }

            int count = 0;
            var results = new List<object>();
            var readableResults = new List<string>();

            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    count++;
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 2) continue;

                    string name = values[0].Trim();
                    string email = values[1].Trim();

                    
                    if (count == 1 && email.ToLower().Contains("email"))
                        continue;

                    string personalizedContent = content.Replace("<>", name);

                    try
                    {
                        await SendEmail(email, subject, personalizedContent);

                        var msg = $"✅ Mail sent to {name} ({email})";
                        results.Add(new { name, email, status = "success", message = msg });
                        readableResults.Add($"<span style='color:green'>{msg}</span>");
                    }
                    catch (Exception e)
                    {
                        var msg = $"❌ Failed to send to {name} ({email}): {e.Message}";
                        results.Add(new { name, email, status = "failed", message = msg });
                        readableResults.Add($"<span style='color:red'>{msg}</span>");
                    }

                    await Task.Delay(500); // Slow down to mimic live sending
                }
            }

            TempData["Results"] = readableResults;
            

            
            return Json(results);
        }

        
        public IActionResult Results()
        {
            var results = TempData["Results"];
            ViewBag.Results = results;
            return View();
        }

        
        private async Task SendEmail(string to, string subject, string body)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            message.From = new MailAddress("ayushca68@gmail.com");
            message.To.Add(to);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;

            smtpClient.Port = 587;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("ayushca68@gmail.com", "bwvv xxed sdll bugg");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            await smtpClient.SendMailAsync(message);
        }
    }
}
