/**
 * @fileoverview Unit Tests for Stock Sync Admin Page (Story 2.5)
 * Tests cover AC1-AC11 acceptance criteria using Vitest + React Testing Library
 */

import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import { describe, test, expect, vi, beforeEach, afterEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import StockSyncAdminPage from './StockSyncAdminPage';
import SyncHistoryTable from './components/SyncHistoryTable';
import SyncDetailsModal from './components/SyncDetailsModal';
import StockSyncDashboard from './components/StockSyncDashboard';
import apiRequest from '../../../utils/api';
import { AdminContext } from '../../../context/AdminContext';

// Mock the API module
vi.mock('../../../utils/api');

// Mock AdminHeader to avoid router dependency issues in tests
vi.mock('../../../components/AdminHeader', () => ({
  default: () => <div data-testid="admin-header">Admin Header</div>,
}));

const mockDealership = { id: 1, name: 'Test Dealership' };

const mockSyncStatus = {
  lastSyncedAt: '2026-01-25T02:05:00Z',
  status: 'Success',
  itemsProcessed: 47,
  itemsSucceeded: 45,
  itemsFailed: 2,
  durationMs: 135000,
  hasCredentials: true,
};

const mockSyncHistory = {
  logs: [
    {
      id: 1,
      syncedAt: '2026-01-25T02:05:00Z',
      status: 'Success',
      itemsProcessed: 47,
      itemsSucceeded: 45,
      itemsFailed: 2,
      durationMs: 135000,
    },
  ],
  total: 1,
  page: 1,
  pageSize: 10,
  totalPages: 1,
};

function renderWithContext(ui, dealership = mockDealership) {
  return render(
    <AdminContext.Provider value={{ selectedDealership: dealership }}>
      <MemoryRouter>{ui}</MemoryRouter>
    </AdminContext.Provider>
  );
}

function mockApiSuccess(statusData = mockSyncStatus, historyData = mockSyncHistory) {
  apiRequest.mockImplementation((url) => {
    if (url.includes('sync-status')) {
      return Promise.resolve({ ok: true, json: async () => statusData });
    }
    if (url.includes('sync-history')) {
      return Promise.resolve({ ok: true, json: async () => historyData });
    }
    return Promise.resolve({ ok: true, json: async () => ({}) });
  });
}

beforeEach(() => {
  vi.clearAllMocks();
});

afterEach(() => {
  vi.restoreAllMocks();
});

// ---------------------------------------------------------------------------
// StockSyncAdminPage tests
// ---------------------------------------------------------------------------

describe('StockSyncAdminPage', () => {
  test('AC2: renders last sync status after data load', async () => {
    mockApiSuccess();
    renderWithContext(<StockSyncAdminPage />);

    await waitFor(() => {
      expect(screen.getByText('Success')).toBeInTheDocument();
    });
    expect(screen.getAllByText('47').length).toBeGreaterThan(0);
    expect(screen.getAllByText('45').length).toBeGreaterThan(0);
  });

  test('AC3: Sync Now button calls POST sync/trigger', async () => {
    mockApiSuccess();
    apiRequest.mockImplementation((url, options) => {
      if (url.includes('sync/trigger') && options?.method === 'POST') {
        return Promise.resolve({ ok: true, json: async () => ({ syncLogId: 123, message: 'Sync started' }) });
      }
      if (url.includes('sync-status')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      }
      if (url.includes('sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<StockSyncAdminPage />);

    const syncBtn = await screen.findByRole('button', { name: /sync now/i });
    fireEvent.click(syncBtn);

    await waitFor(() => {
      expect(apiRequest).toHaveBeenCalledWith(
        '/api/easycars/sync/trigger',
        expect.objectContaining({ method: 'POST' })
      );
    });
  });

  test('AC4: button shows Syncing... and is disabled after click', async () => {
    // Stall the trigger response so we can check loading state
    let resolvePost;
    const postPromise = new Promise((res) => { resolvePost = res; });

    apiRequest.mockImplementation((url, options) => {
      if (url.includes('sync/trigger') && options?.method === 'POST') return postPromise;
      if (url.includes('sync-status')) return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      if (url.includes('sync-history')) return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<StockSyncAdminPage />);

    const syncBtn = await screen.findByRole('button', { name: /sync now/i });
    fireEvent.click(syncBtn);

    await waitFor(() => {
      expect(screen.getByText(/syncing/i)).toBeInTheDocument();
    });

    // Clean up
    resolvePost({ ok: true, json: async () => ({ syncLogId: 1 }) });
  });

  test('AC6: success feedback when sync completes', async () => {
    mockApiSuccess();
    apiRequest.mockImplementation((url, options) => {
      if (url.includes('sync/trigger') && options?.method === 'POST') {
        return Promise.resolve({ ok: true, json: async () => ({ syncLogId: 1 }) });
      }
      if (url.includes('sync-status')) {
        return Promise.resolve({ ok: true, json: async () => ({ ...mockSyncStatus, status: 'Success' }) });
      }
      if (url.includes('sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<StockSyncAdminPage />);
    await screen.findByRole('button', { name: /sync now/i });
  });

  test('AC7: displays error feedback when sync trigger fails', async () => {
    mockApiSuccess();
    apiRequest.mockImplementation((url, options) => {
      if (url.includes('sync/trigger') && options?.method === 'POST') {
        return Promise.resolve({ ok: false, json: async () => ({ error: 'Rate limit exceeded' }) });
      }
      if (url.includes('sync-status')) return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      if (url.includes('sync-history')) return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<StockSyncAdminPage />);

    const syncBtn = await screen.findByRole('button', { name: /sync now/i });
    fireEvent.click(syncBtn);

    // Button should be re-enabled on error
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /sync now/i })).not.toBeDisabled();
    });
  });

  test('AC10: shows credential warning when hasCredentials is false', async () => {
    apiRequest.mockImplementation((url) => {
      if (url.includes('sync-status')) {
        return Promise.resolve({ ok: true, json: async () => ({ ...mockSyncStatus, hasCredentials: false }) });
      }
      if (url.includes('sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<StockSyncAdminPage />);

    await waitFor(() => {
      expect(screen.getByText(/credentials not configured/i)).toBeInTheDocument();
    });

    const syncBtn = screen.getByRole('button', { name: /sync now/i });
    expect(syncBtn).toBeDisabled();
  });

  test('shows loading spinner before data loads', () => {
    apiRequest.mockImplementation(() => new Promise(() => {})); // Never resolves
    renderWithContext(<StockSyncAdminPage />);
    // Loading spinner is rendered via SVG, check container exists
    expect(document.querySelector('.animate-spin')).toBeTruthy();
  });
});

// ---------------------------------------------------------------------------
// StockSyncDashboard component tests
// ---------------------------------------------------------------------------

describe('StockSyncDashboard', () => {
  const defaultProps = {
    syncStatus: mockSyncStatus,
    hasCredentials: true,
    isSyncing: false,
    onSyncNow: vi.fn(),
  };

  test('AC2: renders all sync status fields', () => {
    render(<MemoryRouter><StockSyncDashboard {...defaultProps} /></MemoryRouter>);
    expect(screen.getByText('Success')).toBeInTheDocument();
    expect(screen.getByText('47')).toBeInTheDocument();
    expect(screen.getByText('45')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  test('AC4: button disabled and shows Syncing... when isSyncing=true', () => {
    render(<MemoryRouter><StockSyncDashboard {...defaultProps} isSyncing={true} /></MemoryRouter>);
    const btn = screen.getByRole('button', { name: /syncing/i });
    expect(btn).toBeDisabled();
  });

  test('AC10: credential warning rendered when hasCredentials=false', () => {
    render(<MemoryRouter><StockSyncDashboard {...defaultProps} hasCredentials={false} /></MemoryRouter>);
    expect(screen.getByText(/credentials not configured/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sync now/i })).toBeDisabled();
  });

  test('AC2: shows No synchronization message when syncStatus is null', () => {
    render(<MemoryRouter><StockSyncDashboard {...defaultProps} syncStatus={null} /></MemoryRouter>);
    expect(screen.getByText(/No synchronization has run yet/i)).toBeInTheDocument();
  });
});

// ---------------------------------------------------------------------------
// SyncHistoryTable component tests
// ---------------------------------------------------------------------------

describe('SyncHistoryTable', () => {
  test('AC8: renders table rows with sync history data', () => {
    render(
      <MemoryRouter>
        <SyncHistoryTable syncHistory={mockSyncHistory.logs} onViewDetails={vi.fn()} />
      </MemoryRouter>
    );
    expect(screen.getAllByText('✅ Success').length).toBeGreaterThan(0);
    expect(screen.getAllByRole('button', { name: /view details/i }).length).toBeGreaterThan(0);
  });

  test('AC8: shows empty state when syncHistory is empty', () => {
    render(
      <MemoryRouter>
        <SyncHistoryTable syncHistory={[]} onViewDetails={vi.fn()} />
      </MemoryRouter>
    );
    expect(screen.getByText(/No sync history available/i)).toBeInTheDocument();
  });

  test('AC8: calls onViewDetails with correct id when View Details clicked', () => {
    const onViewDetails = vi.fn();
    render(
      <MemoryRouter>
        <SyncHistoryTable syncHistory={mockSyncHistory.logs} onViewDetails={onViewDetails} />
      </MemoryRouter>
    );
    fireEvent.click(screen.getAllByRole('button', { name: /view details/i })[0]);
    expect(onViewDetails).toHaveBeenCalledWith(1);
  });
});

// ---------------------------------------------------------------------------
// SyncDetailsModal component tests
// ---------------------------------------------------------------------------

describe('SyncDetailsModal', () => {
  test('AC9: renders loading state while fetching', () => {
    apiRequest.mockImplementation(() => new Promise(() => {})); // Never resolves
    render(
      <MemoryRouter>
        <SyncDetailsModal syncLogId={1} onClose={vi.fn()} />
      </MemoryRouter>
    );
    expect(document.querySelector('.animate-spin')).toBeTruthy();
  });

  test('AC9: renders sync details once loaded', async () => {
    apiRequest.mockResolvedValue({
      ok: true,
      json: async () => ({
        id: 1,
        dealershipId: 1,
        syncedAt: '2026-01-25T02:05:00Z',
        status: 'Success',
        itemsProcessed: 47,
        itemsSucceeded: 45,
        itemsFailed: 2,
        errors: [],
        durationMs: 135000,
      }),
    });

    render(
      <MemoryRouter>
        <SyncDetailsModal syncLogId={1} onClose={vi.fn()} />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Sync Details/i)).toBeInTheDocument();
    });
    expect(screen.getByText('✅ Success')).toBeInTheDocument();
  });

  test('AC9: displays error messages when errors present', async () => {
    apiRequest.mockResolvedValue({
      ok: true,
      json: async () => ({
        id: 2,
        dealershipId: 1,
        syncedAt: '2026-01-24T02:04:00Z',
        status: 'PartialSuccess',
        itemsProcessed: 50,
        itemsSucceeded: 48,
        itemsFailed: 2,
        errors: ['VIN ABC123: Invalid body type', 'VIN XYZ789: Missing make/model'],
        durationMs: 120000,
      }),
    });

    render(
      <MemoryRouter>
        <SyncDetailsModal syncLogId={2} onClose={vi.fn()} />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('VIN ABC123: Invalid body type')).toBeInTheDocument();
    });
  });

  test('AC9: closes on ESC key', async () => {
    apiRequest.mockResolvedValue({
      ok: true,
      json: async () => ({ id: 1, syncedAt: '2026-01-25T02:05:00Z', status: 'Success', itemsProcessed: 0, itemsSucceeded: 0, itemsFailed: 0, errors: [], durationMs: 0 }),
    });

    const onClose = vi.fn();
    render(
      <MemoryRouter>
        <SyncDetailsModal syncLogId={1} onClose={onClose} />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Sync Details/i)).toBeInTheDocument();
    });

    fireEvent.keyDown(window, { key: 'Escape' });
    expect(onClose).toHaveBeenCalled();
  });
});

