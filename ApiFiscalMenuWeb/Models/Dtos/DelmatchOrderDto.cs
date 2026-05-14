using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class DelmatchOrderDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("reference")] public int Reference { get; set; }
    [JsonPropertyName("shortReference")] public int ShortReference { get; set; }
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("merchant")] public DelmatchMerchantInfoDto Merchant { get; set; } = new();
    [JsonPropertyName("payments")] public List<DelmatchPaymentDto> Payments { get; set; } = new();
    [JsonPropertyName("customer")] public DelmatchCustomerDto Customer { get; set; } = new();
    [JsonPropertyName("items")] public List<DelmatchItemDto> Items { get; set; } = new();
    [JsonPropertyName("subTotal")] public decimal SubTotal { get; set; }
    [JsonPropertyName("totalPrice")] public decimal TotalPrice { get; set; }
    [JsonPropertyName("deliveryFee")] public decimal DeliveryFee { get; set; }
    [JsonPropertyName("deliveryAddress")] public DelmatchAddressDto DeliveryAddress { get; set; } = new();
    [JsonPropertyName("partner")] public DelmatchPartnerDto? Partner { get; set; }
    [JsonPropertyName("deliveryDateTime")] public string? DeliveryDateTime { get; set; }
    [JsonPropertyName("preparationTimeInSeconds")] public int PreparationTimeInSeconds { get; set; }
    [JsonPropertyName("scheduleDateTime")] public string? ScheduleDateTime { get; set; }
}

public class DelmatchMerchantInfoDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("phones")] public List<string> Phones { get; set; } = new();
    [JsonPropertyName("address")] public DelmatchAddressDto Address { get; set; } = new();
}

public class DelmatchPaymentDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("prepaid")] public bool Prepaid { get; set; }
    [JsonPropertyName("value")] public decimal Value { get; set; }
    [JsonPropertyName("issuer")] public string Issuer { get; set; } = string.Empty;
}

public class DelmatchCustomerDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("taxPayerIdentificationNumber")] public string TaxPayerIdentificationNumber { get; set; } = string.Empty;
    [JsonPropertyName("phone")] public string Phone { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
}

public class DelmatchItemDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("subItemsPrice")] public decimal SubItemsPrice { get; set; }
    [JsonPropertyName("totalPrice")] public decimal TotalPrice { get; set; }
    [JsonPropertyName("discount")] public decimal Discount { get; set; }
    [JsonPropertyName("addition")] public decimal Addition { get; set; }
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("observations")] public string? Observations { get; set; }
    [JsonPropertyName("subItems")] public List<DelmatchSubItemDto> SubItems { get; set; } = new();
}

public class DelmatchSubItemDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("totalPrice")] public decimal TotalPrice { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("discount")] public decimal Discount { get; set; }
    [JsonPropertyName("addition")] public decimal Addition { get; set; }
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("group")] public string Group { get; set; } = string.Empty;
    [JsonPropertyName("group_id")] public string GroupId { get; set; } = string.Empty;
    [JsonPropertyName("type")] public int Type { get; set; }
}

public class DelmatchAddressDto
{
    [JsonPropertyName("formattedAddress")] public string FormattedAddress { get; set; } = string.Empty;
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
    [JsonPropertyName("neighboardhood")] public string? Neighbourhood { get; set; }
    [JsonPropertyName("streetName")] public string StreetName { get; set; } = string.Empty;
    [JsonPropertyName("streetNumber")] public string StreetNumber { get; set; } = string.Empty;
    [JsonPropertyName("postalCode")] public string PostalCode { get; set; } = string.Empty;
    [JsonPropertyName("reference")] public string? Reference { get; set; }
    [JsonPropertyName("complement")] public string? Complement { get; set; }
}

public class DelmatchPartnerDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
}
