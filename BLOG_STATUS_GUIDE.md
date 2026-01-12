# Blog Status Workflow - Important!

## Understanding Blog Post Status

When you create a blog post, it has three possible statuses:

### 1. **Draft** (Default)
- ✅ Visible in Admin CMS (Blog Manager)
- ❌ **NOT visible on public website**
- Use this when you're still working on the post
- Allows you to save your work without publishing

### 2. **Published**
- ✅ Visible in Admin CMS (Blog Manager)
- ✅ **VISIBLE on public website** (/blog page)
- This is what website visitors will see
- Automatically sets the `published_at` timestamp

### 3. **Archived**
- ✅ Visible in Admin CMS (Blog Manager)
- ❌ **NOT visible on public website**
- Use this to hide old posts without deleting them
- Keeps the post for record-keeping

## How to Publish a Blog Post

### When Creating a New Post:
1. Go to `/admin/blogs`
2. Click "Add New Blog Post"
3. Fill in all the details
4. **Important**: In the "Status" dropdown, select **"Published"**
5. Click "Create Blog Post"
6. The post will now appear on the public blog page

### To Publish an Existing Draft:
1. Go to `/admin/blogs`
2. Find your draft post (it will show status as "Draft")
3. Click "Edit"
4. Change the "Status" dropdown to **"Published"**
5. Click "Update Blog Post"
6. The post will now appear on the public blog page

## Troubleshooting

### "I created a blog post but it's not showing on the website"
**Check the status:**
- Go to `/admin/blogs`
- Look at the "Status" column for your post
- If it says "Draft", the post won't be visible to visitors
- Edit the post and change status to "Published"

### "How do I preview a draft without publishing?"
Currently, drafts are only visible in the Admin panel. To preview:
- Keep the status as "Draft"
- View it in the Blog Manager list to see the title and excerpt
- For full preview, you would need to temporarily publish it, view it, then set it back to draft

## Multi-Tenancy Verification

Each blog post belongs to ONE dealership:
- Posts created for "Acme Auto Sales" only appear on Acme's website
- Posts created for "Best Cars" only appear on Best Cars' website
- The `dealership_id` ensures complete isolation between dealerships

## Quick Test

To verify your blog is working:

1. **Create a published post:**
   - Admin Panel → Blog Manager → Add New Blog Post
   - Title: "Welcome to Our Blog"
   - Content: "This is our first post!"
   - Status: **Published** ← Important!
   - Click "Create Blog Post"

2. **View on public site:**
   - Go to the dealership website
   - Click "Blog" in the navigation
   - You should see your post in the grid

3. **Test multi-tenancy:**
   - Switch to a different dealership in Admin
   - Create a blog post for that dealership
   - Visit each dealership's public website
   - Verify each shows only their own posts

## Status Summary Table

| Status    | Admin Visible | Public Visible | Use Case |
|-----------|---------------|----------------|----------|
| Draft     | ✅ Yes        | ❌ No          | Work in progress |
| Published | ✅ Yes        | ✅ **Yes**     | Live on website |
| Archived  | ✅ Yes        | ❌ No          | Old/hidden posts |

---

**Remember:** Only posts with status = "Published" appear on the public website!
