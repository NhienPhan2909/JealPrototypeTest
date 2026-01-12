/**
 * @fileoverview Email notification service.
 * Handles sending email notifications for new leads and sales requests to dealerships.
 *
 * Uses nodemailer for SMTP email delivery.
 *
 * Functions:
 * - sendNewLeadNotification(dealershipEmail, leadData) - Sends email notification for new lead
 * - sendNewSalesRequestNotification(dealershipEmail, salesRequestData) - Sends email notification for new sales request
 */

const nodemailer = require('nodemailer');

/**
 * Creates and configures nodemailer transporter.
 *
 * IMPORTANT: Email credentials must be configured in .env file:
 * - EMAIL_HOST: SMTP server host (e.g., smtp.gmail.com)
 * - EMAIL_PORT: SMTP server port (e.g., 587 for TLS)
 * - EMAIL_USER: Email account username
 * - EMAIL_PASSWORD: Email account password or app-specific password
 * - EMAIL_FROM: Sender email address (e.g., noreply@yourdomain.com)
 *
 * @returns {nodemailer.Transporter} Configured email transporter
 */
function createTransporter() {
  return nodemailer.createTransport({
    host: process.env.EMAIL_HOST,
    port: parseInt(process.env.EMAIL_PORT || '587'),
    secure: process.env.EMAIL_PORT === '465', // true for port 465, false for other ports
    auth: {
      user: process.env.EMAIL_USER,
      pass: process.env.EMAIL_PASSWORD,
    },
  });
}

/**
 * Sends new lead notification email to dealership.
 *
 * Includes lead details: name, email, phone, vehicle (if applicable), and message.
 *
 * @param {string} dealershipEmail - Dealership email address to send notification to
 * @param {Object} leadData - Lead information
 * @param {string} leadData.name - Customer name
 * @param {string} leadData.email - Customer email
 * @param {string} leadData.phone - Customer phone
 * @param {string} leadData.message - Customer message/enquiry
 * @param {string} [leadData.vehicleInfo] - Vehicle information (optional, e.g., "2020 Toyota Camry")
 * @returns {Promise<Object>} Email send result from nodemailer
 * @throws {Error} If email sending fails
 *
 * @example
 * await sendNewLeadNotification('dealer@example.com', {
 *   name: 'John Doe',
 *   email: 'john@example.com',
 *   phone: '(555) 111-2222',
 *   message: 'Interested in this vehicle',
 *   vehicleInfo: '2020 Toyota Camry'
 * });
 */
async function sendNewLeadNotification(dealershipEmail, leadData) {
  // Skip email sending if email configuration is not set (dev/test environment)
  if (!process.env.EMAIL_HOST || !process.env.EMAIL_USER) {
    console.warn('Email configuration not set. Skipping email notification.');
    console.log('Lead data:', leadData);
    return { skipped: true };
  }

  const transporter = createTransporter();

  // Prepare vehicle information display
  const vehicleSection = leadData.vehicleInfo
    ? `<p><strong>Vehicle:</strong> ${leadData.vehicleInfo}</p>`
    : '<p><strong>Vehicle:</strong> General Enquiry</p>';

  // Build HTML email content
  const htmlContent = `
    <html>
      <head>
        <style>
          body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
          }
          .container {
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
          }
          h1 {
            color: #3B82F6;
            border-bottom: 2px solid #3B82F6;
            padding-bottom: 10px;
          }
          .lead-details {
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            margin-top: 20px;
          }
          .lead-details p {
            margin: 10px 0;
          }
          strong {
            color: #555;
          }
        </style>
      </head>
      <body>
        <div class="container">
          <h1>🚗 New Lead Received</h1>
          <p>You have received a new enquiry from a potential customer.</p>

          <div class="lead-details">
            <h2>Lead Details</h2>
            <p><strong>Name:</strong> ${leadData.name}</p>
            <p><strong>Email:</strong> ${leadData.email}</p>
            <p><strong>Phone:</strong> ${leadData.phone}</p>
            ${vehicleSection}
            <p><strong>Message:</strong></p>
            <p>${leadData.message.replace(/\n/g, '<br>')}</p>
          </div>

          <p style="margin-top: 20px; color: #666; font-size: 14px;">
            Please respond to this enquiry as soon as possible to increase your chances of conversion.
          </p>
        </div>
      </body>
    </html>
  `;

  // Build plain text version (fallback for email clients that don't support HTML)
  const textContent = `
New Lead Received

You have received a new enquiry from a potential customer.

Lead Details:
--------------
Name: ${leadData.name}
Email: ${leadData.email}
Phone: ${leadData.phone}
Vehicle: ${leadData.vehicleInfo || 'General Enquiry'}
Message:
${leadData.message}

Please respond to this enquiry as soon as possible to increase your chances of conversion.
  `.trim();

  // Configure email options
  const mailOptions = {
    from: process.env.EMAIL_FROM || process.env.EMAIL_USER,
    to: dealershipEmail,
    subject: `New Lead: ${leadData.name} - ${leadData.vehicleInfo || 'General Enquiry'}`,
    text: textContent,
    html: htmlContent,
  };

  // Send email
  try {
    const info = await transporter.sendMail(mailOptions);
    console.log('New lead notification email sent:', info.messageId);
    return info;
  } catch (error) {
    console.error('Error sending new lead notification email:', error);
    throw error;
  }
}

/**
 * Sends new sales request notification email to dealership.
 *
 * Includes all sales request details: customer info, vehicle details, and additional message.
 *
 * @param {string} dealershipEmail - Dealership email address to send notification to
 * @param {Object} salesRequestData - Sales request information
 * @param {string} salesRequestData.name - Customer name
 * @param {string} salesRequestData.email - Customer email
 * @param {string} salesRequestData.phone - Customer phone
 * @param {string} salesRequestData.make - Vehicle make
 * @param {string} salesRequestData.model - Vehicle model
 * @param {number} salesRequestData.year - Vehicle year
 * @param {number} salesRequestData.kilometers - Vehicle kilometers
 * @param {string} [salesRequestData.additional_message] - Optional additional message
 * @returns {Promise<Object>} Email send result from nodemailer
 * @throws {Error} If email sending fails
 *
 * @example
 * await sendNewSalesRequestNotification('dealer@example.com', {
 *   name: 'Jane Smith',
 *   email: 'jane@example.com',
 *   phone: '(555) 222-3333',
 *   make: 'Honda',
 *   model: 'Civic',
 *   year: 2019,
 *   kilometers: 45000,
 *   additional_message: 'Car is in excellent condition'
 * });
 */
async function sendNewSalesRequestNotification(dealershipEmail, salesRequestData) {
  // Skip email sending if email configuration is not set (dev/test environment)
  if (!process.env.EMAIL_HOST || !process.env.EMAIL_USER) {
    console.warn('Email configuration not set. Skipping email notification.');
    console.log('Sales request data:', salesRequestData);
    return { skipped: true };
  }

  const transporter = createTransporter();

  // Prepare additional message section
  const additionalMessageSection = salesRequestData.additional_message
    ? `<p><strong>Additional Message:</strong></p>
            <p>${salesRequestData.additional_message.replace(/\n/g, '<br>')}</p>`
    : '<p><strong>Additional Message:</strong> None</p>';

  // Build HTML email content
  const htmlContent = `
    <html>
      <head>
        <style>
          body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
          }
          .container {
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
          }
          h1 {
            color: #3B82F6;
            border-bottom: 2px solid #3B82F6;
            padding-bottom: 10px;
          }
          h2 {
            color: #555;
            margin-top: 20px;
          }
          .details-section {
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            margin-top: 20px;
          }
          .details-section p {
            margin: 10px 0;
          }
          strong {
            color: #555;
          }
        </style>
      </head>
      <body>
        <div class="container">
          <h1>🚗💰 New "Sell Your Car" Request</h1>
          <p>You have received a new vehicle sales request from a customer.</p>

          <div class="details-section">
            <h2>Customer Information</h2>
            <p><strong>Name:</strong> ${salesRequestData.name}</p>
            <p><strong>Email:</strong> ${salesRequestData.email}</p>
            <p><strong>Phone:</strong> ${salesRequestData.phone}</p>
          </div>

          <div class="details-section">
            <h2>Vehicle Details</h2>
            <p><strong>Make:</strong> ${salesRequestData.make}</p>
            <p><strong>Model:</strong> ${salesRequestData.model}</p>
            <p><strong>Year:</strong> ${salesRequestData.year}</p>
            <p><strong>Kilometers:</strong> ${salesRequestData.kilometers.toLocaleString()} km</p>
          </div>

          <div class="details-section">
            <h2>Additional Information</h2>
            ${additionalMessageSection}
          </div>

          <p style="margin-top: 20px; color: #666; font-size: 14px;">
            Please contact the customer to discuss their vehicle valuation and purchase options.
          </p>
        </div>
      </body>
    </html>
  `;

  // Build plain text version (fallback for email clients that don't support HTML)
  const textContent = `
New "Sell Your Car" Request

You have received a new vehicle sales request from a customer.

Customer Information:
----------------------
Name: ${salesRequestData.name}
Email: ${salesRequestData.email}
Phone: ${salesRequestData.phone}

Vehicle Details:
-----------------
Make: ${salesRequestData.make}
Model: ${salesRequestData.model}
Year: ${salesRequestData.year}
Kilometers: ${salesRequestData.kilometers.toLocaleString()} km

Additional Information:
------------------------
${salesRequestData.additional_message || 'None'}

Please contact the customer to discuss their vehicle valuation and purchase options.
  `.trim();

  // Configure email options
  const mailOptions = {
    from: process.env.EMAIL_FROM || process.env.EMAIL_USER,
    to: dealershipEmail,
    subject: `New Sales Request: ${salesRequestData.year} ${salesRequestData.make} ${salesRequestData.model}`,
    text: textContent,
    html: htmlContent,
  };

  // Send email
  try {
    const info = await transporter.sendMail(mailOptions);
    console.log('New sales request notification email sent:', info.messageId);
    return info;
  } catch (error) {
    console.error('Error sending new sales request notification email:', error);
    throw error;
  }
}

module.exports = {
  sendNewLeadNotification,
  sendNewSalesRequestNotification,
};
