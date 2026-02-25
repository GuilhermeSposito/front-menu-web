using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace ApiFiscalMenuWeb.Models.Dtos;

public class PedidoIfoodDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("displayId")] public string DisplayId { get; set; } = string.Empty;
    [JsonPropertyName("orderType")] public string OrderType { get; set; } = string.Empty;
    [JsonPropertyName("orderTiming")] public string OrderTiming { get; set; } = string.Empty;
    [JsonPropertyName("salesChannel")] public string SalesChannel { get; set; } = string.Empty;
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]public DateTime CreatedAt { get; set; }
    [JsonPropertyName("preparationStartDateTime")] public DateTime PreparationStart { get; set; }
    [JsonPropertyName("isTeste")] public bool IsTeste { get; set; }
    [JsonPropertyName("extraInfo")] public string ExtraInfo { get; set; } = string.Empty;
    [JsonPropertyName("merchant")] public MerchantIfoodDto Merchant { get; set; } = new MerchantIfoodDto();
    [JsonPropertyName("customer")] public CustomerIfoodDto Customer { get; set; } = new CustomerIfoodDto();
    [JsonPropertyName("items")] public List<ItemIfoodDto> Items { get; set; } = new List<ItemIfoodDto>();
    [JsonPropertyName("benefits")] public List<BenefitsIfoodDto> Benefits { get; set; } = new List<BenefitsIfoodDto>();
    [JsonPropertyName("additionalFees")] public List<AdditionalFeesIfoodDto> AdditionalFees { get; set; } = new List<AdditionalFeesIfoodDto>();
    [JsonPropertyName("total")] public TotalIfoodDto Total { get; set; } = new TotalIfoodDto();
    [JsonPropertyName("payments")] public PaymentIfoodDto Payments { get; set; } = new PaymentIfoodDto();
    [JsonPropertyName("picking")] public PickingIfoodDto Picking { get; set; } = new PickingIfoodDto();
    [JsonPropertyName("delivery")] public DeliveryIfoodDto Delivery { get; set; } = new DeliveryIfoodDto();
    [JsonPropertyName("takeout")] public TakeoutIfoodDto Takeout { get; set; } = new TakeoutIfoodDto();
    [JsonPropertyName("dineIn")] public DineInIfoodDto DineIn { get; set; } = new DineInIfoodDto();
    [JsonPropertyName("indoor")] public IndoorIfoodDto Indoor { get; set; } = new IndoorIfoodDto();
    [JsonPropertyName("scheduled")] public ScheduledIfoodDto Scheduled { get; set; } = new ScheduledIfoodDto();

}

public class MerchantIfoodDto
{
    [JsonPropertyName("id")] public string MerchantId { get; set; } = string.Empty; 
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty; 
} 

public class CustomerIfoodDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("documentNumber")] public string DocumentNumber { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("ordersCountOnMerchant")] public int OrdersCountOnMerchant { get; set; }
    [JsonPropertyName("phone")] public PhoneIfoodDto Phone { get; set; } = new PhoneIfoodDto();

}
public class PhoneIfoodDto
{
    [JsonPropertyName("number")] public string Number { get; set; } = string.Empty;
    [JsonPropertyName("localizer")] public string Localizer { get; set; } = string.Empty;
    [JsonPropertyName("localizerExpiration")] public DateTime LocalizerExpiration { get; set; }
}

public class ItemIfoodDto 
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty; 
    [JsonPropertyName("uniqueId")] public string UniqueId { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; } = string.Empty; 
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("ean")] public string Ean { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public double Quantity { get; set; }
    [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
    [JsonPropertyName("unitPrice")] public double UnitPrice { get; set; }
    [JsonPropertyName("price")] public double Price{ get; set; }
    [JsonPropertyName("scalePrices")] public List<ScalePricesIfoodDto> ScalePrices { get; set; } = new List<ScalePricesIfoodDto>();
    [JsonPropertyName("optionsPrice")] public double OptionsPrice { get; set; }
    [JsonPropertyName("customizationPrice")] public double CustomizationPrice { get; set; }
    [JsonPropertyName("totalPrice")] public double TotalPrice { get; set; }
    [JsonPropertyName("observations")] public string Observations { get; set; } = string.Empty;
    [JsonPropertyName("options")] public List<OptionsIfoodDto> Options { get; set; } = new List<OptionsIfoodDto>();



}
public class ScalePricesIfoodDto
{
    [JsonPropertyName("defaultPrice")] public double DefaultPrice { get; set; }
    [JsonPropertyName("scales")] public List<ScalesIfoodDto> Scales { get; set; } = new List<ScalesIfoodDto>();
}

public class ScalesIfoodDto
{
    [JsonPropertyName("price")] public double Price { get; set; }
    [JsonPropertyName("minQuantity")] public double MinQuantity { get; set; }
}

public class OptionsIfoodDto
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty; 
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("groupName")] public string GroupName { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public double Quantity { get; set; }
    [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
    [JsonPropertyName("unitPrice")] public double UnitPrice { get; set; }
    [JsonPropertyName("addition")] public double Addition { get; set; }
    [JsonPropertyName("price")] public double Price { get; set; }
    [JsonPropertyName("customizations")] public List<OptionsCustomizationsIfoodDto> Customizations { get; set; } = new List<OptionsCustomizationsIfoodDto>();
}

public class OptionsCustomizationsIfoodDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("groupName")] public string GroupName { get; set; } = string.Empty;
    [JsonPropertyName("externalCode")] public string ExternalCode { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public double Quantity { get; set; }
    [JsonPropertyName("unitPrice")] public double UnitPrice { get; set; }
    [JsonPropertyName("addition")] public double Addition { get; set; }
    [JsonPropertyName("price")] public double Price { get; set; }

}

public class BenefitsIfoodDto
{
    [JsonPropertyName("value")] public double Value { get; set; } 
    [JsonPropertyName("target")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("targetId")] public string TargetId { get; set; } = string.Empty;
    [JsonPropertyName("sponsorshipValues")] public List<SponsorShipValuesIfoodDto> Description { get; set; } = new List<SponsorShipValuesIfoodDto>();
    [JsonPropertyName("campaign")] public CampaignIfoodDto Campaign { get; set; } = new CampaignIfoodDto();
}

public class SponsorShipValuesIfoodDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("value")] public double Value { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}

public class CampaignIfoodDto
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
}

public class AdditionalFeesIfoodDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("fullDescription")] public string FullDescription { get; set; } = string.Empty;
    [JsonPropertyName("value")] public double Value { get; set; }
    [JsonPropertyName("liabilities")] public List<LiabilitiesIfoodDto> Liabilities { get; set; } = new List<LiabilitiesIfoodDto>();
}
public class LiabilitiesIfoodDto
{
    [JsonPropertyName("name")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("percentage")] public double Percentage { get; set; }

}

public class TotalIfoodDto
{
    [JsonPropertyName("subTotal")] public double SubTotal { get; set; }
    [JsonPropertyName("deliveryFee")] public double DeliveryFee { get; set; }
    [JsonPropertyName("additionalFees")] public double AddtionalFees { get; set; }
    [JsonPropertyName("benefits")] public double Benefits { get; set; }
    [JsonPropertyName("orderAmount")] public double OrderAmount { get; set; }
}

public class PaymentIfoodDto
{
    [JsonPropertyName("prepaid")] public double Prepaid { get; set; }
    [JsonPropertyName("pending")] public double Pending { get; set; }
    [JsonPropertyName("methods")] public List<MethodsIfoodDto> Methods { get; set; } = new List<MethodsIfoodDto>();
}

public class MethodsIfoodDto
{
    [JsonPropertyName("value")] public double Value { get; set; }
    [JsonPropertyName("currency")] public string Currency { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("method")] public string Method { get; set; } = string.Empty;
    [JsonPropertyName("card")] public CardMethodsIfoodDto Card { get; set; } = new CardMethodsIfoodDto();
    [JsonPropertyName("walletName")] public WalletNameMethodsIfoodDto WalletName { get; set; } = new WalletNameMethodsIfoodDto();
    [JsonPropertyName("cash")] public CashMethodsIfoodDto Cash { get; set; } = new CashMethodsIfoodDto();
    [JsonPropertyName("transaction")] public TransactionMethodsIfoodDto Transaction { get; set; } = new TransactionMethodsIfoodDto();
}

public class CardMethodsIfoodDto
{
    [JsonPropertyName("brand")] public string Brand { get; set; } = string.Empty;
}

public class WalletNameMethodsIfoodDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

}

public class CashMethodsIfoodDto
{
    [JsonPropertyName("changeFor")] public double ChangeFor { get; set; }
}
public class TransactionMethodsIfoodDto
{
    [JsonPropertyName("authorizationCode")] public string AuthorizationCode { get; set; } = string.Empty;
    [JsonPropertyName("acquirerDocument")] public string AcquirerDocument { get; set; } = string.Empty;
}

public class PickingIfoodDto
{
    [JsonPropertyName("picker")] public string Picker { get; set; } = string.Empty;
    [JsonPropertyName("replacementOptions")] public string ReplacementOptions { get; set; } = string.Empty;
}

public class DeliveryIfoodDto
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("deliveredBy")] public string DeliveredBy { get; set; } = string.Empty;
    [JsonPropertyName("deliveryDateTime")] public DateTime DeliveryDateTime { get; set; }
    [JsonPropertyName("observations")] public string Observations { get; set; } = string.Empty;
    [JsonPropertyName("deliveryAddress")] public DeliveryAddresIfoodDto DeliveryAddress { get; set; } = new DeliveryAddresIfoodDto();
    [JsonPropertyName("pickupCode")] public string PickupCode { get; set; } = string.Empty;
}

public class DeliveryAddresIfoodDto
{
    [JsonPropertyName("streetName")] public string StreetName { get; set; } = string.Empty;
    [JsonPropertyName("streetNumber")] public string StreetNumber { get; set; } = string.Empty;
    [JsonPropertyName("formattedAddress")] public string FormattedAddress { get; set; } = string.Empty;
    [JsonPropertyName("neighborhood")] public string Neighborhood { get; set; } = string.Empty;
    [JsonPropertyName("complement")] public string Complement { get; set; } = string.Empty;
    [JsonPropertyName("reference")] public string Reference { get; set; } = string.Empty;
    [JsonPropertyName("postalCode")] public string PostalCode { get; set; } = string.Empty;
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
    [JsonPropertyName("coordinates")] public CoordinatesIfoodDto Coordinates { get; set; } = new CoordinatesIfoodDto();


}

public class CoordinatesIfoodDto
{
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
}

public class TakeoutIfoodDto
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = string.Empty;
    [JsonPropertyName("takeoutDateTime")]public DateTime TakeoutDateTime { get; set; }
    [JsonPropertyName("observations")] public string Observations { get; set; } = string.Empty;
}

public class DineInIfoodDto
{
  [JsonPropertyName("deliveryDateTime")] public DateTime DeliveryDateTime { get; set; }
}

public class IndoorIfoodDto
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = string.Empty;
    [JsonPropertyName("table")] public string Table { get; set; } = string.Empty;
    [JsonPropertyName("deliveryDateTime")] public DateTime DeliveryDateTime { get; set; }
    [JsonPropertyName("observations")] public string Observations { get; set; } = string.Empty;
}


public class ScheduledIfoodDto
{
    [JsonPropertyName("deliveryDateTimeStart")] public DateTime DeliveryDateTimeStart { get; set; }
    [JsonPropertyName("deliveryDateTimeEnd")] public DateTime DeliveryDateTimeEnd { get; set; }
}

