import { FormEvent, useState } from 'react';
import { ApiError } from '../../../shared/api/http';
import { useAdjustWallet } from '../hooks/useUsers';
import type { AdjustmentReason, UserDetails } from '../types';

interface Props {
  user: UserDetails;
}

const reasons: AdjustmentReason[] = ['Topup', 'Refund', 'Correction', 'Purchase', 'Chargeback'];

interface FormState {
  amount: string;
  reason: AdjustmentReason;
  note: string;
}

const initial: FormState = { amount: '', reason: 'Topup', note: '' };

export function WalletAdjustmentForm({ user }: Props) {
  const [form, setForm] = useState<FormState>(initial);
  const [validation, setValidation] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const mutation = useAdjustWallet();

  function handleChange<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
    setValidation(null);
    setSuccessMessage(null);
  }

  function onSubmit(event: FormEvent) {
    event.preventDefault();
    setValidation(null);
    setSuccessMessage(null);
    mutation.reset();

    const parsed = Number.parseFloat(form.amount);
    if (Number.isNaN(parsed) || parsed === 0) {
      setValidation('Enter a non-zero amount. Use a negative number to deduct.');
      return;
    }

    const projected = user.walletBalance + parsed;
    if (projected < 0) {
      setValidation(`Deduction would take the balance below zero (current $${user.walletBalance.toFixed(2)}).`);
      return;
    }

    mutation.mutate(
      {
        userId: user.id,
        payload: {
          amount: parsed,
          reason: form.reason,
          note: form.note.trim() || undefined
        },
        idempotencyKey: crypto.randomUUID()
      },
      {
        onSuccess: (result) => {
          setSuccessMessage(
            `Adjustment recorded. New balance: $${result.balanceAfter.toFixed(2)}${
              result.wasReplayed ? ' (replay detected).' : '.'
            }`
          );
          setForm(initial);
        }
      }
    );
  }

  const serverError = mutation.error
    ? mutation.error instanceof ApiError
      ? mutation.error.detail ?? mutation.error.message
      : (mutation.error as Error).message
    : null;

  return (
    <form onSubmit={onSubmit} className="wallet-form" aria-labelledby="adjust-heading">
      <h2 id="adjust-heading">Adjust wallet balance</h2>
      <p>
        Current balance: <strong>${user.walletBalance.toFixed(2)}</strong>
      </p>

      <label>
        <span>Amount (negative to deduct)</span>
        <input
          type="number"
          step="0.01"
          value={form.amount}
          onChange={(e) => handleChange('amount', e.target.value)}
          required
          aria-invalid={validation ? true : undefined}
          aria-describedby={validation ? 'amount-error' : undefined}
        />
      </label>

      <label>
        <span>Reason</span>
        <select value={form.reason} onChange={(e) => handleChange('reason', e.target.value as AdjustmentReason)}>
          {reasons.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
      </label>

      <label>
        <span>Note (optional)</span>
        <textarea
          value={form.note}
          maxLength={512}
          rows={3}
          onChange={(e) => handleChange('note', e.target.value)}
        />
      </label>

      {validation && (
        <p role="alert" id="amount-error" className="feedback feedback--error">
          {validation}
        </p>
      )}
      {serverError && (
        <p role="alert" className="feedback feedback--error">
          {serverError}
        </p>
      )}
      {successMessage && (
        <p role="status" className="feedback feedback--success">
          {successMessage}
        </p>
      )}

      <button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? 'Saving...' : 'Apply adjustment'}
      </button>
    </form>
  );
}
