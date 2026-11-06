/**
 * Minimal toast queue. Replaces the ad-hoc `message` refs rendered as bare
 * <p> tags in quotations and dispatches.
 *
 * Usage:
 *   const toast = useToast()
 *   toast.success('Dispatch issued')
 *   toast.error(err)
 */
export type ToastTone = 'success' | 'error' | 'info' | 'warning'

export interface Toast {
  id: number
  tone: ToastTone
  message: string
  timeout: number
}

let nextId = 0

export function useToast() {
  const toasts = useState<Toast[]>('toasts', () => [])

  function dismiss(id: number) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  function push(tone: ToastTone, message: unknown, timeout = 4000) {
    const text =
      message instanceof Error
        ? (message as Error & { data?: { message?: string } }).data?.message || message.message
        : String(message)

    const id = ++nextId
    toasts.value = [...toasts.value, { id, tone, message: text, timeout }]

    if (timeout > 0 && typeof window !== 'undefined') {
      window.setTimeout(() => dismiss(id), timeout)
    }
    return id
  }

  return {
    toasts,
    dismiss,
    push,
    success: (m: unknown, t?: number) => push('success', m, t),
    error: (m: unknown, t?: number) => push('error', m, t ?? 6000),
    info: (m: unknown, t?: number) => push('info', m, t),
    warning: (m: unknown, t?: number) => push('warning', m, t)
  }
}
