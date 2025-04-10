import { useEffect } from 'react'
import { useGlobal, useTabLayout } from './'

export const useDocumentTitle = (pathname, title) => {
  const { config } = useGlobal()
  const { currentTab } = useTabLayout()

  title = `${title} - ${config.siteName}`

  useEffect(() => {
    if (currentTab === pathname) {
      document.title = title
    }
  }, [title, currentTab, pathname])
}
