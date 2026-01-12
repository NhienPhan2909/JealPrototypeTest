/**
 * Test script for Lead Status and Delete functionality
 * Tests:
 * 1. Create a test lead
 * 2. Update lead status from 'received' to 'in progress'
 * 3. Update lead status to 'done'
 * 4. Delete the lead
 */

const BASE_URL = 'http://localhost:5000';

async function runTests() {
  console.log('🧪 Testing Lead Status and Delete Functionality\n');
  
  let leadId;
  const dealershipId = 1; // Using dealership ID 1 for testing

  try {
    // Test 1: Create a test lead
    console.log('📝 Test 1: Creating test lead...');
    const createResponse = await fetch(`${BASE_URL}/api/leads`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        dealership_id: dealershipId,
        name: 'Test Customer',
        email: 'test@example.com',
        phone: '555-1234',
        message: 'This is a test enquiry for status tracking'
      })
    });

    if (!createResponse.ok) {
      throw new Error(`Failed to create lead: ${createResponse.status}`);
    }

    const createdLead = await createResponse.json();
    leadId = createdLead.id;
    console.log(`✅ Lead created with ID: ${leadId}`);
    console.log(`   Initial status: ${createdLead.status || 'received'}\n`);

    // Test 2: Fetch leads and verify the new lead exists
    console.log('📋 Test 2: Fetching leads to verify creation...');
    const fetchResponse = await fetch(`${BASE_URL}/api/leads?dealershipId=${dealershipId}`);
    
    if (!fetchResponse.ok) {
      throw new Error(`Failed to fetch leads: ${fetchResponse.status}`);
    }

    const leads = await fetchResponse.json();
    const foundLead = leads.find(l => l.id === leadId);
    
    if (!foundLead) {
      throw new Error('Created lead not found in list');
    }
    
    console.log(`✅ Lead found in list with status: ${foundLead.status}\n`);

    // Test 3: Update lead status to 'in progress'
    console.log('🔄 Test 3: Updating status to "in progress"...');
    const updateToInProgressResponse = await fetch(
      `${BASE_URL}/api/leads/${leadId}/status?dealershipId=${dealershipId}`,
      {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          status: 'in progress'
        })
      }
    );

    if (!updateToInProgressResponse.ok) {
      const error = await updateToInProgressResponse.text();
      throw new Error(`Failed to update status to in progress: ${updateToInProgressResponse.status} - ${error}`);
    }

    const updatedLead1 = await updateToInProgressResponse.json();
    console.log(`✅ Status updated to: ${updatedLead1.status}\n`);

    // Test 4: Update lead status to 'done'
    console.log('✅ Test 4: Updating status to "done"...');
    const updateToDoneResponse = await fetch(
      `${BASE_URL}/api/leads/${leadId}/status?dealershipId=${dealershipId}`,
      {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          status: 'done'
        })
      }
    );

    if (!updateToDoneResponse.ok) {
      const error = await updateToDoneResponse.text();
      throw new Error(`Failed to update status to done: ${updateToDoneResponse.status} - ${error}`);
    }

    const updatedLead2 = await updateToDoneResponse.json();
    console.log(`✅ Status updated to: ${updatedLead2.status}\n`);

    // Test 5: Verify status update persists
    console.log('🔍 Test 5: Verifying status persists...');
    const verifyResponse = await fetch(`${BASE_URL}/api/leads?dealershipId=${dealershipId}`);
    
    if (!verifyResponse.ok) {
      throw new Error(`Failed to fetch leads for verification: ${verifyResponse.status}`);
    }

    const leadsAfterUpdate = await verifyResponse.json();
    const verifiedLead = leadsAfterUpdate.find(l => l.id === leadId);
    
    if (!verifiedLead) {
      throw new Error('Lead not found after status update');
    }
    
    if (verifiedLead.status !== 'done') {
      throw new Error(`Expected status 'done', got '${verifiedLead.status}'`);
    }
    
    console.log(`✅ Status correctly persisted as: ${verifiedLead.status}\n`);

    // Test 6: Test invalid status value
    console.log('🚫 Test 6: Testing invalid status value...');
    const invalidStatusResponse = await fetch(
      `${BASE_URL}/api/leads/${leadId}/status?dealershipId=${dealershipId}`,
      {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          status: 'invalid_status'
        })
      }
    );

    if (invalidStatusResponse.ok) {
      throw new Error('Expected invalid status to be rejected, but request succeeded');
    }
    
    console.log(`✅ Invalid status correctly rejected with status: ${invalidStatusResponse.status}\n`);

    // Test 7: Delete the lead
    console.log('🗑️  Test 7: Deleting test lead...');
    const deleteResponse = await fetch(
      `${BASE_URL}/api/leads/${leadId}?dealershipId=${dealershipId}`,
      {
        method: 'DELETE'
      }
    );

    if (!deleteResponse.ok) {
      const error = await deleteResponse.text();
      throw new Error(`Failed to delete lead: ${deleteResponse.status} - ${error}`);
    }

    console.log(`✅ Lead deleted successfully\n`);

    // Test 8: Verify lead is deleted
    console.log('🔍 Test 8: Verifying lead is deleted...');
    const finalFetchResponse = await fetch(`${BASE_URL}/api/leads?dealershipId=${dealershipId}`);
    
    if (!finalFetchResponse.ok) {
      throw new Error(`Failed to fetch leads for final verification: ${finalFetchResponse.status}`);
    }

    const finalLeads = await finalFetchResponse.json();
    const deletedLeadCheck = finalLeads.find(l => l.id === leadId);
    
    if (deletedLeadCheck) {
      throw new Error('Lead still exists after deletion');
    }
    
    console.log(`✅ Lead successfully removed from database\n`);

    // Test 9: Test multi-tenancy security (try to delete with wrong dealership ID)
    console.log('🔒 Test 9: Testing multi-tenancy security...');
    
    // Create another lead for this test
    const testLead = await fetch(`${BASE_URL}/api/leads`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        dealership_id: dealershipId,
        name: 'Security Test',
        email: 'security@example.com',
        phone: '555-9999',
        message: 'Security test lead'
      })
    });
    const securityTestLead = await testLead.json();
    
    // Try to delete with wrong dealership ID
    const wrongDealershipDelete = await fetch(
      `${BASE_URL}/api/leads/${securityTestLead.id}?dealershipId=99999`,
      { method: 'DELETE' }
    );

    if (wrongDealershipDelete.ok) {
      throw new Error('Security breach: Lead deleted with wrong dealership ID!');
    }
    
    console.log(`✅ Multi-tenancy security working (status: ${wrongDealershipDelete.status})\n`);
    
    // Clean up the security test lead
    await fetch(
      `${BASE_URL}/api/leads/${securityTestLead.id}?dealershipId=${dealershipId}`,
      { method: 'DELETE' }
    );

    console.log('🎉 All tests passed successfully!\n');

  } catch (error) {
    console.error('❌ Test failed:', error.message);
    
    // Cleanup: try to delete test lead if it exists
    if (leadId) {
      try {
        await fetch(
          `${BASE_URL}/api/leads/${leadId}?dealershipId=${dealershipId}`,
          { method: 'DELETE' }
        );
        console.log('\n🧹 Cleaned up test lead');
      } catch (cleanupError) {
        console.log('\n⚠️  Could not clean up test lead:', cleanupError.message);
      }
    }
    
    process.exit(1);
  }
}

// Run tests
runTests();
