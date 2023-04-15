using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class WebScraperAndPdfGenerator : MonoBehaviour
{
    public TMP_InputField urlInputField;
    public TMP_InputField targetStringInputField;
    public TMP_InputField outputFilenameInputField;
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(ProcessInputs);
    }

    private IEnumerator WebScraping(string url, string targetString, System.Action<List<string>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var html = request.downloadHandler.text;
                var document = new HtmlDocument();
                document.LoadHtml(html);

                var scrapedText = document.DocumentNode.DescendantsAndSelf()
                    .Where(n => n.NodeType == HtmlNodeType.Text && n.InnerText.Contains(targetString))
                    .Select(n => n.InnerText)
                    .ToList();

                callback(scrapedText);
            }
            else
            {
                Debug.LogError($"Error fetching data: {request.error}");
            }
        }
    }

    private void GeneratePdf(List<string> textList, string outputFilename, string fontPath)
    {
        BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, 12);

        using (var stream = new FileStream(outputFilename, FileMode.Create))
        {
            Document document = new Document(PageSize.LETTER, 50, 50, 50, 50);
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = Element.ALIGN_LEFT;

            foreach (string line in textList)
            {
                PdfPCell cell = new PdfPCell(new Phrase(line.Trim(), font));
                cell.Border = PdfPCell.NO_BORDER;
                table.AddCell(cell);
            }

            document.Add(table);
            document.Close();
        }
    }

    private void ProcessInputs()
    {
        string url = urlInputField.text;
        string targetString = targetStringInputField.text;
        string outputFilename = outputFilenameInputField.text;

        string fontPath = "D:/temp/TakaoGothic.ttf";

        StartCoroutine(WebScraping(url, targetString, scrapedText =>
        {
            if (scrapedText.Count > 0)
            {
                GeneratePdf(scrapedText, outputFilename, fontPath);
                Debug.Log($"PDFファイル'{outputFilename}'が生成されました。");
            }
            else
            {
                Debug.LogWarning("指定された文字列が見つかりませんでした。");
            }
        }));
    }
}
