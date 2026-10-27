namespace ReadyErp.Api.Services;

public record LineInput(decimal Quantity, decimal UnitPrice, decimal LineDiscountPercent, decimal LineDiscountAmount);

public record TotalsResult(decimal SubTotal, decimal Discount, decimal Charges, decimal Vat, decimal GrandTotal);

public static class PricingCalculator
{
    public static decimal LineNet(LineInput line)
    {
        var amount = line.Quantity * line.UnitPrice;
        amount -= amount * (line.LineDiscountPercent / 100m);
        amount -= line.LineDiscountAmount;
        return Math.Max(0, Math.Round(amount, 2));
    }

    public static TotalsResult Compute(
        IEnumerable<LineInput> lines,
        decimal documentDiscountPercent,
        decimal documentDiscountAmount,
        decimal vatPercent,
        decimal serviceCharge,
        decimal weightCharge,
        decimal deliveryCharge)
    {
        var subTotal = lines.Sum(LineNet);

        var afterDiscount = subTotal;
        afterDiscount -= subTotal * (documentDiscountPercent / 100m);
        afterDiscount -= documentDiscountAmount;
        afterDiscount = Math.Max(0, afterDiscount);

        var charges = serviceCharge + weightCharge + deliveryCharge;

        // Charges added before VAT (default assumed in proposal, Q7).
        var vatBase = afterDiscount + charges;
        var vat = Math.Round(vatBase * (vatPercent / 100m), 2);

        var grand = Math.Round(afterDiscount + charges + vat, 2);
        var discount = Math.Round(subTotal - afterDiscount, 2);

        return new TotalsResult(Math.Round(subTotal, 2), discount, Math.Round(charges, 2), vat, grand);
    }
}
