import useSWR from 'swr'
import useSWRInfinite from 'swr/infinite'
import useSWRMutation from 'swr/mutation'
import { apiClient } from '@/services/api/client'
import { useAuthStore } from '@/stores'

// Generic fetcher function
export const fetcher = async <T>(url: string): Promise<T> => {
  const response = await apiClient.get<T>(url)
  return response.data.data
}

// Generic mutation function
export const mutateFetcher = async <T, Args = unknown>(
  url: string,
  { arg }: { arg: Args }
): Promise<T> => {
  const response = await apiClient.post<T>(url, arg)
  return response.data.data
}

// Hook for fetching data with SWR
export function useApiData<T>(
  url: string | null,
  options?: {
    refreshInterval?: number
    revalidateOnFocus?: boolean
    revalidateOnReconnect?: boolean
    dedupingInterval?: number
  }
) {
  const { isAuthenticated } = useAuthStore()

  return useSWR<T>(
    isAuthenticated && url ? url : null,
    fetcher<T>,
    {
      revalidateOnFocus: false,
      revalidateOnReconnect: true,
      dedupingInterval: 5000,
      ...options,
    }
  )
}

// Hook for infinite loading (pagination)
export function useInfiniteApiData<T>(
  getKey: (index: number, previousPageData: T | null) => string | null,
  options?: {
    refreshInterval?: number
    revalidateOnFocus?: boolean
  }
) {
  const { isAuthenticated } = useAuthStore()

  return useSWRInfinite<T>(
    (index, previousPageData) => {
      if (!isAuthenticated) return null
      return getKey(index, previousPageData)
    },
    fetcher<T>,
    {
      revalidateOnFocus: false,
      revalidateOnReconnect: true,
      ...options,
    }
  )
}

// Hook for mutations (POST, PUT, DELETE)
export function useApiMutation<TData = unknown, Args = unknown>(
  url: string,
  options?: {
    onSuccess?: (data: TData) => void
    onError?: (error: Error) => void
  }
) {
  const { isAuthenticated } = useAuthStore()

  const mutation = useSWRMutation<TData, Error, string | null, Args>(
    isAuthenticated ? url : null,
    mutateFetcher<TData, Args>,
    {
      onSuccess: options?.onSuccess,
      onError: options?.onError,
    }
  )

  return {
    ...mutation,
    trigger: async (args: Args) => {
      try {
        // @ts-expect-error trigger type conflict with nullable key
        const result = await mutation.trigger(args)
        return result
      } catch (error) {
        if (options?.onError) {
          options.onError(error as Error)
        }
        throw error
      }
    },
  }
}

// Hook for optimistic updates
export function useOptimisticUpdate<T>(
  key: string,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  updateFn: (currentData: T, newData: Partial<T>) => T
) {
  return useSWR<T>(key, fetcher<T>, {
    onSuccess: (data) => {
      // Custom optimistic update logic can be added here
      return data
    },
  })
}
