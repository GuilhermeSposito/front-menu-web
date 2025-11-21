using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class RetunApiRefatored
{
    public string status { get; set; } = "success";
    public List<string> message { get; set; } = new List<string>();

    [JsonIgnore] public Data data { get; set; } = new Data();
}

public class Data
{
    public List<string> message { get; set; } = new List<string>();
}
