<script setup lang="ts">
/**
 * Shared printable document body for invoices and dispatch notes.
 *
 * Only the item table columns and the totals block differ between the two, so
 * those are slots. Everything else — letterhead, bill-to block, metadata,
 * signature row — lives here once.
 *
 * Colours are kept literal rather than tokenised: this renders on paper, where
 * the surface/on-surface roles are meaningless and contrast must be guaranteed.
 */
defineProps<{
  /** e.g. "INVOICE" or "DISPATCH NOTE" */
  kind: string
  number: string
  date: string
  customerName: string
  billingAddress?: string | null
  contactPerson?: string | null
  phone?: string | null
  vatBin?: string | null
  workRef?: string | null
  projectSite?: string | null
  poNo?: string | null
  poDate?: string | null
  jobRef?: string | null
  /** Red "VOIDED" / amber "PENDING" overlay label. */
  watermark?: string | null
}>()

const meta = [
  ['Work Ref', 'workRef'],
  ['Project Site', 'projectSite'],
  ['PO No', 'poNo'],
  ['Job Ref', 'jobRef']
] as const
</script>

<template>
  <article class="relative bg-white p-space-xl text-[#0f172a] print:p-0">
    <div
      v-if="watermark"
      class="pointer-events-none absolute inset-0 flex items-center justify-center"
      aria-hidden="true"
    >
      <span class="rotate-[-24deg] text-7xl font-bold tracking-widest text-[#e11d48]/10">
        {{ watermark }}
      </span>
    </div>

    <!-- Letterhead -->
    <header class="flex items-start justify-between gap-space-lg border-b-2 border-[#2563eb] pb-space-md">
      <div class="flex items-center gap-space-sm">
        <span
          class="flex size-11 shrink-0 items-center justify-center rounded-xl bg-[#2563eb] text-white"
        >
          <Icon name="material-symbols:monitor-heart-outline" class="size-6" />
        </span>
        <div>
          <h1 class="font-headline-lg text-headline-lg text-[#0f172a]">E&amp;G Engineering Ltd.</h1>
          <p class="font-body-sm text-body-sm text-[#64748b]">
            Gensets, compressors, automation &amp; solar power
          </p>
        </div>
      </div>

      <div class="text-right">
        <h2 class="font-headline-md text-headline-md uppercase tracking-wide text-[#2563eb]">
          {{ kind }}
        </h2>
        <p class="font-label-lg text-label-lg tabular text-[#0f172a]">{{ number }}</p>
        <p class="font-body-sm text-body-sm text-[#64748b]">{{ formatDate(date) }}</p>
      </div>
    </header>

    <!-- Parties -->
    <section class="my-space-lg flex flex-wrap justify-between gap-space-lg">
      <div class="min-w-56">
        <p class="mb-1 font-label-sm text-label-sm uppercase tracking-wider text-[#64748b]">
          Bill to
        </p>
        <p class="font-headline-sm text-headline-sm text-[#0f172a]">{{ customerName }}</p>
        <p v-if="billingAddress" class="font-body-sm text-body-sm text-[#334155]">
          {{ billingAddress }}
        </p>
        <p v-if="contactPerson || phone" class="font-body-sm text-body-sm text-[#334155]">
          {{ [contactPerson, phone].filter(Boolean).join(' · ') }}
        </p>
        <p v-if="vatBin" class="font-body-sm text-body-sm text-[#334155]">VAT/BIN: {{ vatBin }}</p>
      </div>

      <dl class="grid grid-cols-[auto_auto] gap-x-space-md gap-y-0.5 font-body-sm text-body-sm">
        <template v-for="[label, key] in meta" :key="key">
          <dt v-if="$props[key]" class="text-[#64748b]">{{ label }}</dt>
          <dd v-if="$props[key]" class="text-right font-medium text-[#0f172a]">
            {{ $props[key] }}
          </dd>
        </template>
        <template v-if="poDate">
          <dt class="text-[#64748b]">PO Date</dt>
          <dd class="text-right font-medium text-[#0f172a]">{{ formatDate(poDate) }}</dd>
        </template>
      </dl>
    </section>

    <!-- Items -->
    <slot name="items" />

    <!-- Totals / notes -->
    <slot name="totals" />

    <!-- Signatures -->
    <footer class="mt-16 flex justify-between gap-space-xl break-inside-avoid">
      <span class="w-44 border-t border-[#334155] pt-2 text-center font-body-sm text-body-sm">
        Prepared By
      </span>
      <span class="w-44 border-t border-[#334155] pt-2 text-center font-body-sm text-body-sm">
        Authorised By
      </span>
      <span class="w-44 border-t border-[#334155] pt-2 text-center font-body-sm text-body-sm">
        Received By
      </span>
    </footer>
  </article>
</template>
