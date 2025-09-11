# Email Notification Testing Guide

## Configuration Setup

1. **Update Email Settings in appsettings.json:**

   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SmtpUsername": "your-email@gmail.com",
     "SmtpPassword": "your-app-password",
     "FromEmail": "your-email@gmail.com",
     "FromName": "Issue Tracking System"
   }
   ```

2. **For Gmail, you need to:**
   - Enable 2-Factor Authentication
   - Generate an App Password (not your regular password)
   - Use the App Password in the SmtpPassword field

## Testing Steps

1. **Start the API:**

   ```bash
   dotnet run
   ```

2. **Create a test ticket:**

   - Use the frontend or API to create a ticket as a regular user
   - Note the ticket ID

3. **Assign and close the ticket:**

   - Login as a Rep user
   - Assign the ticket to yourself
   - Change the status to "Closed"

4. **Check email:**
   - The user who created the ticket should receive an email notification
   - Check both inbox and spam folder

## Email Template Features

The email includes:

- Professional HTML and plain text versions
- Ticket details (ID, title, status, priority, type)
- Creation and closure timestamps
- Assigned representative name
- Resolution notes (if provided)
- Final comment (if provided)
- Responsive design with proper styling

## Troubleshooting

- Check API logs for email sending errors
- Verify SMTP credentials are correct
- Ensure firewall allows SMTP connections
- Check that the ticket creator's email is valid
