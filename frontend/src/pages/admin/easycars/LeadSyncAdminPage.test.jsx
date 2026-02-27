/**
 * @fileoverview Unit Tests for Lead Sync Admin Page (Story 3.5)
 * Tests cover acceptance criteria using Vitest + React Testing Library
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, test, expect, vi, beforeEach, afterEach } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import LeadSyncAdminPage from './LeadSyncAdminPage';
import LeadSyncDashboard from './components/LeadSyncDashboard';
import SyncHistoryTable from './components/SyncHistoryTable';
import SyncDetailsModal from './components/SyncDetailsModal';
import apiRequest from '../../../utils/api';
import { AdminContext } from '../../../context/AdminContext';

// Mock the API module
vi.mock('../../../utils/api');

// Mock AdminHeader to avoid router dependency issues in tests
vi.mock('../../../components/AdminHeader', () => ({
  default: () => <div data-testid="admin-header">Admin Header</div>,
}));

// Mock DealershipContext for AC11 real AdminHeader test
vi.mock('../../../context/DealershipContext', () => ({
  useDealershipContext: () => ({ currentDealershipId: null, setCurrentDealershipId: vi.fn() }),
  DealershipProvider: ({ children }) => children,
}));

const mockDealership = { id: 1, name: 'Test Dealership' };

const mockSyncStatus = {
  lastSyncedAt: '2026-01-25T02:05:00Z',
  status: 'Success',
  itemsProcessed: 12,
  itemsSucceeded: 10,
  itemsFailed: 2,
  durationMs: 95000,
  hasCredentials: true,
};

const mockSyncHistory = {
  logs: [
    {
      id: 1,
      syncedAt: '2026-01-25T02:05:00Z',
      status: 'Success',
      itemsProcessed: 12,
      itemsSucceeded: 10,
      itemsFailed: 2,
      durationMs: 95000,
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
    if (url.includes('lead-sync-status')) {
      return Promise.resolve({ ok: true, json: async () => statusData });
    }
    if (url.includes('lead-sync-history')) {
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
// LeadSyncAdminPage tests
// ---------------------------------------------------------------------------

describe('LeadSyncAdminPage', () => {
  test('renders loading state initially', () => {
    apiRequest.mockImplementation(() => new Promise(() => {})); // Never resolves
    renderWithContext(<LeadSyncAdminPage />);
    expect(document.querySelector('.animate-spin')).toBeTruthy();
  });

  test('renders dashboard with sync status after data loads', async () => {
    mockApiSuccess();
    renderWithContext(<LeadSyncAdminPage />);

    await waitFor(() => {
      expect(screen.getByText('Success')).toBeInTheDocument();
    });
    expect(screen.getAllByText('12').length).toBeGreaterThan(0);
    expect(screen.getAllByText('10').length).toBeGreaterThan(0);
  });

  test('displays no-credentials warning when hasCredentials is false', async () => {
    apiRequest.mockImplementation((url) => {
      if (url.includes('lead-sync-status')) {
        return Promise.resolve({ ok: true, json: async () => ({ ...mockSyncStatus, hasCredentials: false }) });
      }
      if (url.includes('lead-sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<LeadSyncAdminPage />);

    await waitFor(() => {
      expect(screen.getByText(/credentials not configured/i)).toBeInTheDocument();
    });
  });

  test('sync button is disabled when no credentials', async () => {
    apiRequest.mockImplementation((url) => {
      if (url.includes('lead-sync-status')) {
        return Promise.resolve({ ok: true, json: async () => ({ ...mockSyncStatus, hasCredentials: false }) });
      }
      if (url.includes('lead-sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<LeadSyncAdminPage />);

    await waitFor(() => {
      const syncBtn = screen.getByRole('button', { name: /sync now/i });
      expect(syncBtn).toBeDisabled();
    });
  });

  test('sync button triggers POST to lead-sync/trigger on click', async () => {
    mockApiSuccess();
    apiRequest.mockImplementation((url, options) => {
      if (url.includes('lead-sync/trigger') && options?.method === 'POST') {
        return Promise.resolve({ ok: true, json: async () => ({ message: 'Lead sync started', jobId: '123' }) });
      }
      if (url.includes('lead-sync-status')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      }
      if (url.includes('lead-sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<LeadSyncAdminPage />);

    const syncBtn = await screen.findByRole('button', { name: /sync now/i });
    fireEvent.click(syncBtn);

    await waitFor(() => {
      expect(apiRequest).toHaveBeenCalledWith(
        expect.stringContaining('lead-sync/trigger'),
        expect.objectContaining({ method: 'POST' })
      );
    });
  });

  test('sync button shows loading state while isSyncing is true', async () => {
    let resolvePost;
    const postPromise = new Promise((res) => { resolvePost = res; });

    apiRequest.mockImplementation((url, options) => {
      if (url.includes('lead-sync/trigger') && options?.method === 'POST') return postPromise;
      if (url.includes('lead-sync-status')) return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      if (url.includes('lead-sync-history')) return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<LeadSyncAdminPage />);

    const syncBtn = await screen.findByRole('button', { name: /sync now/i });
    fireEvent.click(syncBtn);

    await waitFor(() => {
      expect(screen.getByText(/syncing/i)).toBeInTheDocument();
    });

    // Clean up
    resolvePost({ ok: true, json: async () => ({ jobId: '1' }) });
  });

  test('renders sync history table with log entries', async () => {
    mockApiSuccess();
    renderWithContext(<LeadSyncAdminPage />);

    await waitFor(() => {
      expect(screen.getAllByRole('button', { name: /view details/i }).length).toBeGreaterThan(0);
    });  });

  test('opens sync details modal on View Details click', async () => {
    mockApiSuccess();
    apiRequest.mockImplementation((url) => {
      if (url.includes('lead-sync-status')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncStatus });
      }
      if (url.includes('lead-sync-history')) {
        return Promise.resolve({ ok: true, json: async () => mockSyncHistory });
      }
      if (url.includes('lead-sync-logs')) {
        return Promise.resolve({
          ok: true,
          json: async () => ({
            id: 1,
            dealershipId: 1,
            syncedAt: '2026-01-25T02:05:00Z',
            status: 'Success',
            itemsProcessed: 12,
            itemsSucceeded: 10,
            itemsFailed: 2,
            errors: [],
            durationMs: 95000,
          }),
        });
      }
      return Promise.resolve({ ok: true, json: async () => ({}) });
    });

    renderWithContext(<LeadSyncAdminPage />);

    const viewBtns = await screen.findAllByRole('button', { name: /view details/i });
    fireEvent.click(viewBtns[0]);

    await waitFor(() => {
      expect(screen.getByText(/Sync Details/i)).toBeInTheDocument();
    });
  });
});

// ---------------------------------------------------------------------------
// LeadSyncDashboard component tests
// ---------------------------------------------------------------------------

describe('LeadSyncDashboard', () => {
  const defaultProps = {
    syncStatus: mockSyncStatus,
    hasCredentials: true,
    isSyncing: false,
    onSyncNow: vi.fn(),
  };

  test('renders all sync status fields', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} /></MemoryRouter>);
    expect(screen.getByText('Success')).toBeInTheDocument();
    expect(screen.getByText('12')).toBeInTheDocument();
    expect(screen.getByText('10')).toBeInTheDocument();
  });

  test('shows direction badge', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} /></MemoryRouter>);
    expect(screen.getByText(/Inbound — EasyCars → Local/i)).toBeInTheDocument();
  });

  test('button disabled and shows Syncing... when isSyncing=true', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} isSyncing={true} /></MemoryRouter>);
    const btn = screen.getByRole('button', { name: /syncing/i });
    expect(btn).toBeDisabled();
  });

  test('credential warning rendered when hasCredentials=false', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} hasCredentials={false} /></MemoryRouter>);
    expect(screen.getByText(/credentials not configured/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sync now/i })).toBeDisabled();
  });

  test('shows No synchronization message when syncStatus is null', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} syncStatus={null} /></MemoryRouter>);
    expect(screen.getByText(/No synchronization has run yet/i)).toBeInTheDocument();
  });

  test('shows Leads Processed label instead of Vehicles', () => {
    render(<MemoryRouter><LeadSyncDashboard {...defaultProps} /></MemoryRouter>);
    expect(screen.getByText('Leads Processed')).toBeInTheDocument();
  });
});

// ---------------------------------------------------------------------------
// SyncDetailsModal apiBasePath prop test
// ---------------------------------------------------------------------------

describe('SyncDetailsModal with apiBasePath', () => {
  test('uses apiBasePath prop in API call', async () => {
    apiRequest.mockResolvedValue({
      ok: true,
      json: async () => ({
        id: 1,
        dealershipId: 1,
        syncedAt: '2026-01-25T02:05:00Z',
        status: 'Success',
        itemsProcessed: 5,
        itemsSucceeded: 5,
        itemsFailed: 0,
        errors: [],
        durationMs: 50000,
      }),
    });

    render(
      <MemoryRouter>
        <SyncDetailsModal
          syncLogId={1}
          dealershipId={1}
          onClose={vi.fn()}
          apiBasePath="/api/easycars/lead-sync-logs"
        />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(apiRequest).toHaveBeenCalledWith(
        expect.stringContaining('/api/easycars/lead-sync-logs/1')
      );
    });
  });
});

// ---------------------------------------------------------------------------
// AC11: Navigation and LeadInbox badge integration tests
// ---------------------------------------------------------------------------

describe('AC11 Navigation and LeadInbox badge', () => {
  test('AdminHeader contains Lead Sync nav link to /admin/easycars/lead-sync', async () => {
    // Set up API mock so AdminHeader's dealership fetch doesn't throw
    apiRequest.mockResolvedValue({ ok: true, json: async () => [] });

    // Use importActual to bypass the module-level mock and get the real AdminHeader
    const { default: RealAdminHeader } = await vi.importActual('../../../components/AdminHeader');
    const fullContext = {
      selectedDealership: mockDealership,
      setSelectedDealership: vi.fn(),
      user: { userType: 'admin' },
      setIsAuthenticated: vi.fn(),
    };
    render(
      <AdminContext.Provider value={fullContext}>
        <MemoryRouter>
          <RealAdminHeader />
        </MemoryRouter>
      </AdminContext.Provider>
    );
    const link = await screen.findByRole('link', { name: /lead sync/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', '/admin/easycars/lead-sync');
  });

  test('LeadInbox shows EasyCars badge when lead has easyCarsLeadNumber', async () => {
    const mockLeadsWithEasyCars = [
      {
        id: 1,
        name: 'Jane Smith',
        email: 'jane@example.com',
        phone: '0400000000',
        message: 'Interested in this car',
        status: 'New',
        easyCarsLeadNumber: 'LEAD-001',
        vehicleId: null,
        createdAt: '2026-01-25T02:05:00Z',
      },
    ];

    apiRequest.mockImplementation(() =>
      Promise.resolve({ ok: true, json: async () => mockLeadsWithEasyCars })
    );

    const { default: LeadInbox } = await import('../LeadInbox');
    render(
      <AdminContext.Provider value={{ selectedDealership: mockDealership }}>
        <MemoryRouter>
          <LeadInbox />
        </MemoryRouter>
      </AdminContext.Provider>
    );

    await waitFor(() => {
      expect(screen.getByText('EasyCars')).toBeInTheDocument();
    });
  });
});
