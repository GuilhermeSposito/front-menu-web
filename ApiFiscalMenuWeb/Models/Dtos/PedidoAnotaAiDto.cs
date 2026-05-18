using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class PedidoAnotaAiDto
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("info")] public AnotaAiOrderInfoDto Info { get; set; } = new();
}

public class AnotaAiOrderInfoDto
{
    [JsonPropertyName("_id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("id")] public string IdAlt { get; set; } = string.Empty;
    [JsonPropertyName("check")] public int Check { get; set; }
    [JsonPropertyName("deliveryFee")] public decimal DeliveryFee { get; set; }
    [JsonPropertyName("menu_version")] public int MenuVersion { get; set; }
    [JsonPropertyName("observation")] public string? Observation { get; set; }
    [JsonPropertyName("preparationStartDateTime")] public string? PreparationStartDateTime { get; set; }
    [JsonPropertyName("qr_description")] public string? QrDescription { get; set; }
    [JsonPropertyName("salesChannel")] public string SalesChannel { get; set; } = string.Empty;
    [JsonPropertyName("shortReference")] public int ShortReference { get; set; }
    [JsonPropertyName("time_max")] public string? TimeMax { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("ifood_id")] public string? IfoodId { get; set; }
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = string.Empty;
    [JsonPropertyName("updatedAt")] public string UpdatedAt { get; set; } = string.Empty;
    [JsonPropertyName("order_automatic_accept")] public bool OrderAutomaticAccept { get; set; }
    [JsonPropertyName("additionalFees")] public List<AnotaAiAdditionalFeeDto> AdditionalFees { get; set; } = new();
    [JsonPropertyName("customer")] public AnotaAiCustomerDto Customer { get; set; } = new();
    [JsonPropertyName("deliveryAddress")] public AnotaAiDeliveryAddressDto? DeliveryAddress { get; set; }
    [JsonPropertyName("discounts")] public List<AnotaAiDiscountDto> Discounts { get; set; } = new();
    [JsonPropertyName("items")] public List<AnotaAiItemDto> Items { get; set; } = new();
    [JsonPropertyName("merchant")] public AnotaAiMerchantDto Merchant { get; set; } = new();
    [JsonPropertyName("payments")] public List<AnotaAiPaymentDto> Payments { get; set; } = new();
    [JsonPropertyName("pdv")] public AnotaAiPdvDto? Pdv { get; set; }
}

public class AnotaAiAdditionalFeeDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("value")] public decimal Value { get; set; }
}

public class AnotaAiCustomerDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("taxPayerIdentificationNumber")] public string? TaxPayerIdentificationNumber { get; set; }
}

public class AnotaAiDeliveryAddressDto
{
    [JsonPropertyName("formattedAddress")] public string FormattedAddress { get; set; } = string.Empty;
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
    [JsonPropertyName("coordinates")] public AnotaAiCoordinatesDto? Coordinates { get; set; }
    [JsonPropertyName("neighborhood")] public string? Neighborhood { get; set; }
    [JsonPropertyName("streetName")] public string StreetName { get; set; } = string.Empty;
    [JsonPropertyName("streetNumber")] public string StreetNumber { get; set; } = string.Empty;
    [JsonPropertyName("postalCode")] public string PostalCode { get; set; } = string.Empty;
    [JsonPropertyName("reference")] public string? Reference { get; set; }
    [JsonPropertyName("complement")] public string? Complement { get; set; }
    [JsonPropertyName("ifood_pickup_code")] public string? IfoodPickupCode { get; set; }
    [JsonPropertyName("ifood_return_code")] public string? IfoodReturnCode { get; set; }
    [JsonPropertyName("ifood_verification_code")] public string? IfoodVerificationCode { get; set; }
}

public class AnotaAiCoordinatesDto
{
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
}

public class AnotaAiDiscountDto
{
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("tag")] public string Tag { get; set; } = string.Empty;
}

public class AnotaAiItemDto
{
    [JsonPropertyName("_id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("id")] public int IdAlt { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("internalId")] public string InternalId { get; set; } = string.Empty;
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("subItems")] public List<AnotaAiSubItemDto> SubItems { get; set; } = new();
}

public class AnotaAiSubItemDto
{
    [JsonPropertyName("_id")] public string? MongoId { get; set; }
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("id_parent")] public int? IdParent { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("internalId")] public string? InternalId { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("totalPrice")] public decimal? TotalPrice { get; set; }
    [JsonPropertyName("unitPrice")] public decimal? UnitPrice { get; set; }
    [JsonPropertyName("new_totalPrice")] public decimal? NewTotalPrice { get; set; }
    [JsonPropertyName("new_unitPrice")] public decimal? NewUnitPrice { get; set; }
    [JsonPropertyName("quantityFraction")] public decimal? QuantityFraction { get; set; }
    [JsonPropertyName("valueFraction")] public decimal? ValueFraction { get; set; }
}

public class AnotaAiMerchantDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("restaurantId")] public string? RestaurantId { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
}

public class AnotaAiPaymentDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("value")] public decimal Value { get; set; }
    [JsonPropertyName("cardSelected")] public string CardSelected { get; set; } = string.Empty;
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("changeFor")] public decimal? ChangeFor { get; set; }
    [JsonPropertyName("prepaid")] public bool Prepaid { get; set; }
}

public class AnotaAiPdvDto
{
    [JsonPropertyName("status")] public bool Status { get; set; }
    [JsonPropertyName("mode")] public int? Mode { get; set; }
    [JsonPropertyName("table")] public string? Table { get; set; }
    [JsonPropertyName("ticket")] public string? Ticket { get; set; }
}
