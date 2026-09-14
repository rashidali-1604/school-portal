import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '../api/usersApi';
import type {
  UserSearchParams,
  WalletAdjustmentPayload,
  WalletAdjustmentResult
} from '../types';

export const userKeys = {
  all: ['users'] as const,
  list: (params: UserSearchParams) => [...userKeys.all, 'list', params] as const,
  detail: (id: string) => [...userKeys.all, 'detail', id] as const
};

export function useUsers(params: UserSearchParams) {
  return useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => usersApi.search(params),
    placeholderData: (previous) => previous
  });
}

export function useUser(id: string | undefined) {
  return useQuery({
    queryKey: id ? userKeys.detail(id) : ['user', 'noop'],
    queryFn: () => usersApi.getById(id as string),
    enabled: !!id
  });
}

export interface AdjustWalletVars {
  userId: string;
  payload: WalletAdjustmentPayload;
  idempotencyKey: string;
}

export function useAdjustWallet() {
  const client = useQueryClient();
  return useMutation<WalletAdjustmentResult, unknown, AdjustWalletVars>({
    mutationFn: ({ userId, payload, idempotencyKey }) =>
      usersApi.adjustWallet(userId, payload, idempotencyKey),
    onSuccess: (_, variables) => {
      client.invalidateQueries({ queryKey: userKeys.detail(variables.userId) });
      client.invalidateQueries({ queryKey: userKeys.all });
    }
  });
}
