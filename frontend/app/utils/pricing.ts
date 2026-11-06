import type { QuotationItem } from '~/types/models'

/**
 * Client-side mirror of backend/Services/PricingCalculator.cs.
 *
 * Used only to preview totals while editing — the server remains the source of
 * truth and recomputes on save. Keep the two in sync: charges are added to the
 * discounted subtotal BEFORE VAT is applied.
 *
 * Note: C# uses `decimal`, JavaScript uses binary floating point, so the two
 * can differ by a fraction of a currency unit on pathological inputs. Each
 * step is rounded to 2dp the same way the server does to keep that minimal.
 */

function round2(value: number) {
  return Math.round((value + Number.EPSILON) * 100) / 100
}

export interface PricingTotals {
  subTotal: number
  discount: number
  charges: number
  vat: number
  grandTotal: number
}

export interface ChargeInput {
  documentDiscountPercent: number
  documentDiscountAmount: number
  vatPercent: number
  serviceCharge: number
  weightCharge: number
  deliveryCharge: number
}

/** Net value of a single line after its own discounts. Never negative. */
export function lineNet(line: Pick<QuotationItem, 'quantity' | 'unitPrice' | 'lineDiscountPercent' | 'lineDiscountAmount'>) {
  let amount = (line.quantity || 0) * (line.unitPrice || 0)
  amount -= amount * ((line.lineDiscountPercent || 0) / 100)
  amount -= line.lineDiscountAmount || 0
  return Math.max(0, round2(amount))
}

export function computeTotals(lines: QuotationItem[], charges: ChargeInput): PricingTotals {
  const subTotal = lines.reduce((sum, line) => sum + lineNet(line), 0)

  let afterDiscount = subTotal
  afterDiscount -= subTotal * ((charges.documentDiscountPercent || 0) / 100)
  afterDiscount -= charges.documentDiscountAmount || 0
  afterDiscount = Math.max(0, afterDiscount)

  const totalCharges =
    (charges.serviceCharge || 0) + (charges.weightCharge || 0) + (charges.deliveryCharge || 0)

  const vatBase = afterDiscount + totalCharges
  const vat = round2(vatBase * ((charges.vatPercent || 0) / 100))

  return {
    subTotal: round2(subTotal),
    discount: round2(subTotal - afterDiscount),
    charges: round2(totalCharges),
    vat,
    grandTotal: round2(afterDiscount + totalCharges + vat)
  }
}
