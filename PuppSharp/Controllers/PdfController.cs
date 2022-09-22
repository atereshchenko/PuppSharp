using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PuppSharp.Controllers
{
    [Route("api/[controller]")]
    public class PdfController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Model model)
        {
            string outputFile = @$"c:\newPDFfile";
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();
            var tmp = await page.GoToAsync(model.url);           
            string nameFile = outputFile + $"-{DateTime.Today.ToString("dd-MM-yyyy HH-MM")}.pdf";
            await page.PdfAsync(nameFile);


            var res = new
            {
                message = $"The file was created successfully {nameFile}",
            };
            return Ok(res);
        }

        [HttpPut]
        public async Task<IActionResult> Put(IFormFile file)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            await using var page = await browser.NewPageAsync();
            await page.EmulateMediaTypeAsync(MediaType.Screen);            
            string stext = file.ReadAsString();            
            await page.SetContentAsync(stext);

            var pdfContent = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });
            return File(pdfContent, "application/pdf", "converted.pdf");            
        }
    }
    public static class Extensions
    {
        public static string ReadAsString(this IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadToEnd());
            }
            return result.ToString();
        }
    }

    public class Model
    {
        public string url { get; set; }
    }    
}
