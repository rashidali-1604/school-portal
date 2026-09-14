import { http } from '../../../shared/api/http';
import type {
  PagedResponse,
  UserDetails,
  UserListItem,
  UserSearchParams,
  WalletAdjustmentPayload,
  WalletAdjustmentResult
} from '../types';

export const usersApi = {
  search(params: UserSearchParams): Promise<PagedResponse<UserListItem>> {
    const query = new URLSearchParams();
    if (params.q) query.set('q', params.q);
    if (params.role) query.set('role', params.role);
    if (params.status) query.set('status', params.status);
    if (params.page) query.set('page', String(params.page));
    if (params.pageSize) query.set('pageSize', String(params.pageSize));
    const qs = query.toString();
    return http.get<PagedResponse<UserListItem>>(`/users${qs ? `?${qs}` : ''}`);
  },

  getById(id: string): Promise<UserDetails> {
    return http.get<UserDetails>(`/users/${id}`);
  },

  adjustWallet(
    id: string,
    payload: WalletAdjustmentPayload,
    idempotencyKey: string
  ): Promise<WalletAdjustmentResult> {
    return http.post<WalletAdjustmentResult>(
      `/users/${id}/wallet-adjustments`,
      payload,
      { headers: { 'Idempotency-Key': idempotencyKey } }
    );
  }
};
