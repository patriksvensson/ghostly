Title: Query Language
Order: 20
Description: Describes GQL, Ghostly's query language
---

To get the most out of Ghostly, you can use the Ghostly Query Language 
(shortened GQL) to search among your pull request, issues, releases, 
vulnerabilities and commits.

*All queries made with GQL are local and does not require an internet connection.*

### Examples

<table class="table">
  <tbody><tr>
    <th width="150">Query</th>
    <th width="250">Explanation</th>
  </tr>
  <tr>
    <td>
        <code>(org:microsoft OR org:cake-build) AND is:pullrequest AND is:open</code>
    </td>
    <td>
        Return all open pull requests that originiated in a repository belonging to 
        either the <code>microsoft</code> or <code>cake-build</code> organisation.
    </td>
  </tr>
  <tr>
    <td>
        <code>is:pullrequest AND is:open AND !muted AND mentions:patriksvensson</code>
    </td>
    <td>
        Return all open pullrequests that you haven't muted that mentions the user 
        <code>patriksvensson</code> in the comment.
    </td>
  </tr>
</tbody>
</table>