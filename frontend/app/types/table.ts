/** Column descriptor for <DataTable>. */
export interface TableColumn {
  /** Property name on the row, also the dynamic slot key (`#cell-<key>`). */
  key: string
  label: string
  align?: 'left' | 'right' | 'center'
  /** Tailwind width class, e.g. 'w-32'. */
  width?: string
  /** Render monospaced-aligned numerals. */
  numeric?: boolean
  /** Hide below the given breakpoint, e.g. 'md' -> hidden until md. */
  hideBelow?: 'sm' | 'md' | 'lg' | 'xl'
}

/** Filter tab descriptor for <TableToolbar>. */
export interface TableTab {
  value: string
  label: string
  count?: number
  icon?: string
}
