import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { usersApi } from '../../api/usersApi';
import { ApiError } from '../../../../shared/api/http';
import { WalletAdjustmentForm } from '../WalletAdjustmentForm';
import type { UserDetails } from '../../types';

vi.mock('../../api/usersApi', () => ({
  usersApi: {
    adjustWallet: vi.fn()
  }
}));

const baseUser: UserDetails = {
  id: 'u-1',
  firstName: 'A',
  lastName: 'B',
  email: 'a@b.com',
  role: 'Parent',
  status: 'Active',
  walletBalance: 50,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  recentAdjustments: []
};

function renderForm(user = baseUser) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={client}>
      <WalletAdjustmentForm user={user} />
    </QueryClientProvider>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  Object.defineProperty(globalThis, 'crypto', {
    value: { randomUUID: () => '00000000-0000-4000-8000-000000000000' },
    configurable: true
  });
});

describe('WalletAdjustmentForm', () => {
  it('shows a client-side validation error when the deduction would overdraw', async () => {
    renderForm({ ...baseUser, walletBalance: 10 });

    await userEvent.type(screen.getByLabelText(/Amount/i), '-25');
    await userEvent.click(screen.getByRole('button', { name: /apply/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent(/below zero/i);
    expect(usersApi.adjustWallet).not.toHaveBeenCalled();
  });

  it('sends the mutation with an idempotency key and reports success', async () => {
    (usersApi.adjustWallet as ReturnType<typeof vi.fn>).mockResolvedValue({
      adjustmentId: 'adj-1',
      userId: 'u-1',
      amount: 25,
      balanceAfter: 75,
      occurredAt: new Date().toISOString(),
      wasReplayed: false
    });

    renderForm();

    await userEvent.type(screen.getByLabelText(/Amount/i), '25');
    await userEvent.selectOptions(screen.getByLabelText(/Reason/i), 'Topup');
    await userEvent.click(screen.getByRole('button', { name: /apply/i }));

    expect(await screen.findByRole('status')).toHaveTextContent(/new balance: \$75\.00/i);
    expect(usersApi.adjustWallet).toHaveBeenCalledWith(
      'u-1',
      expect.objectContaining({ amount: 25, reason: 'Topup' }),
      '00000000-0000-4000-8000-000000000000'
    );
  });

  it('shows the server error detail when the mutation fails', async () => {
    (usersApi.adjustWallet as ReturnType<typeof vi.fn>).mockRejectedValue(
      new ApiError(422, 'Business rule violated', 'Wallet balance 10.00 is insufficient for -25.00.', 'wallet.insufficient_funds')
    );

    renderForm();

    await userEvent.type(screen.getByLabelText(/Amount/i), '20');
    await userEvent.click(screen.getByRole('button', { name: /apply/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent(/insufficient/i);
  });

  it('rejects zero amount before hitting the server', async () => {
    renderForm();
    await userEvent.type(screen.getByLabelText(/Amount/i), '0');
    await userEvent.click(screen.getByRole('button', { name: /apply/i }));
    expect(await screen.findByRole('alert')).toHaveTextContent(/non-zero amount/i);
    expect(usersApi.adjustWallet).not.toHaveBeenCalled();
  });
});
