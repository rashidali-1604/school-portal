export type UserRole = 'Parent' | 'Student' | 'Staff' | 'Administrator';
export type UserStatus = 'Active' | 'Suspended' | 'Closed';
export type AdjustmentReason = 'Topup' | 'Refund' | 'Correction' | 'Purchase' | 'Chargeback';

export interface UserListItem {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  status: UserStatus;
  walletBalance: number;
}

export interface WalletAdjustmentEntry {
  id: string;
  amount: number;
  balanceAfter: number;
  reason: AdjustmentReason;
  note?: string;
  performedBy?: string;
  occurredAt: string;
}

export interface UserDetails extends UserListItem {
  createdAt: string;
  updatedAt: string;
  recentAdjustments: WalletAdjustmentEntry[];
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface WalletAdjustmentPayload {
  amount: number;
  reason: AdjustmentReason;
  note?: string;
}

export interface WalletAdjustmentResult {
  adjustmentId: string;
  userId: string;
  amount: number;
  balanceAfter: number;
  occurredAt: string;
  wasReplayed: boolean;
}

export interface UserSearchParams {
  q?: string;
  role?: UserRole;
  status?: UserStatus;
  page?: number;
  pageSize?: number;
}
