Title: Properties
Order: 3
Description: Explains the different available query properties Ghostly
---

Properties are fields that you can use in your queries or filters.

<table class="table">
  <tbody><tr>
    <th>Property</th>
    <th>Example</th>
  </tr>
  <tr>
    <td><code>archived</code><br /><small>Boolean</small></td>
    <td><code>is:archived</code><br /><i>Returns all archived items</i></td>
  </tr>
  <tr>
    <td><code>assigned</code>, <code>assignee</code><br /><small>Text</small></td>
    <td><code>assigned=patriksvensson</code><br /><i>Returns all items assigned to "patriksvensson"</i></td>
  </tr>
  <tr>
    <td><code>author</code><br /><small>Text</small></td>
    <td><code>author=patriksvensson</code><br /><i>Returns all items created by "patriksvensson"</i></td>
  </tr>
  <tr>
    <td><code>body</code><br /><small>Text</small></td>
    <td><code>body:"foo bar"</code><br /><i>Returns all items containing the text "foo bar" in the body</i></td>
  </tr>
  <tr>
    <td><code>category</code><br /><small>Text</small></td>
    <td><code>category=bots</code><br /><i>Returns all items in the Ghostly category "bots"</i></td>
  </tr>
  <tr>
    <td><code>closed</code><br /><small>Boolean</small></td>
    <td><code>is:closed</code><br /><i>Returns all closed items</i></td>
  </tr>
  <tr>
    <td><code>commenter</code><br /><small>Text</small></td>
    <td><code>commenter=patriksvensson</code><br /><i>Returns all items "patriksvensson" commented on</i></td>
  </tr>
  <tr>
    <td><code>comment</code><br /><small>Text</small></td>
    <td><code>comment:"bug"</code><br /><i>Returns all items with a comment containing the word "bug"</i></td>
  </tr>
  <tr>
    <td><code>commit</code><br /><small>Boolean</small></td>
    <td><code>is:commit</code><br /><i>Returns all commits</i></td>
  </tr>
  <tr>
    <td><code>draft</code><br /><small>Boolean</small></td>
    <td><code>is:draft</code><br /><i>Returns all pull requests that are drafts</i></td>
  </tr>
  <tr>
    <td><code>fork</code><br /><small>Boolean</small></td>
    <td><code>is:fork</code><br /><i>Returns all items that belongs to a fork of another repository</i></td>
  </tr>
  <tr>
    <td><code>id</code><br /><small>Integer</small></td>
    <td><code>id=1234</code><br /><i>Returns the item with the specific external id</i></td>
  </tr>
  <tr>
    <td><code>inbox</code><br /><small>Boolean</small></td>
    <td><code>is:inbox</code><br /><i>Returns all items in the inbox</i></td>
  </tr>
  <tr>
    <td><code>involved</code>, <code>involves</code><br /><small>Text</small></td>
    <td><code>involved=patriksvensson</code><br /><i>Returns all items were "patriksvensson" is involved</i></td>
  </tr>
  <tr>
    <td><code>issue</code><br /><small>Boolean</small></td>
    <td><code>is:issue</code><br /><i>Returns all issues</i></td>
  </tr>
  <tr>
    <td><code>label</code><br /><small>Boolean</small></td>
    <td><code>label:"foo"</code><br /><i>Returns all items with the label "foo"</i></td>
  </tr>
  <tr>
    <td><code>locked</code><br /><small>Boolean</small></td>
    <td><code>is:locked</code><br /><i>Returns all items thats been locked by a moderator</i></td>
  </tr>
  <tr>
    <td><code>mention</code>, <code>mentions</code><br /><small>Text</small></td>
    <td><code>mentions=patriksvensson</code><br /><i>Returns all items were "patriksvensson" has been mentioned</i></td>
  </tr>
  <tr>
    <td><code>merged</code><br /><small>Boolean</small></td>
    <td><code>is:merged</code><br /><i>Returns all pull requests thats been merged</i></td>
  </tr>
  <tr>
    <td><code>merger</code><br /><small>Text</small></td>
    <td><code>merger=patriksvensson</code><br /><i>Returns all pull requests that was merged by "patriksvensson"</i></td>
  </tr>
  <tr>
    <td><code>milestone</code><br /><small>Text</small></td>
    <td><code>milestone="1.0"</code><br /><i>Returns all items belonging to milestone "1.0"</i></td>
  </tr>
  <tr>
    <td><code>muted</code><br /><small>Boolean</small></td>
    <td><code>is:muted</code><br /><i>Returns all muted items</i></td>
  </tr>
  <tr>
    <td><code>open</code><br /><small>Boolean</small></td>
    <td><code>is:open</code><br /><i>Returns all open items</i></td>
  </tr>
  <tr>
    <td><code>owner</code>, <code>org</code><br /><small>Text</small></td>
    <td><code>owner:spectresystems</code><br /><i>Returns all items belonging to the organization or user "spectresystems"</i></td>
  </tr>
  <tr>
    <td><code>private</code><br /><small>Boolean</small></td>
    <td><code>is:private</code><br /><i>Returns all items belonging to a private repository</i></td>
  </tr>
  <tr>
    <td><code>pullrequest</code>, <code>pr</code><br /><small>Boolean</small></td>
    <td><code>is:pr</code><br /><i>Returns all pull requests</i></td>
  </tr>
  <tr>
    <td><code>read</code><br /><small>Boolean</small></td>
    <td><code>is:read</code><br /><i>Returns all read items</i></td>
  </tr>
  <tr>
    <td><code>release</code><br /><small>Boolean</small></td>
    <td><code>is:release</code><br /><i>Returns all releases</i></td>
  </tr>
  <tr>
    <td><code>reopened</code><br /><small>Boolean</small></td>
    <td><code>is:reopened</code><br /><i>Returns all reopened items</i></td>
  </tr>
  <tr>
    <td><code>repository</code>, <code>repo</code><br /><small>Text</small></td>
    <td><code>repo=ghostly</code><br /><i>Returns all items belonging to a repository called "ghostly"</i></td>
  </tr>
  <tr>
    <td><code>reviewer</code><br /><small>Text</small></td>
    <td><code>reviewer=patriksvensson</code><br /><i>Returns all pull requests that "patriksvensson" reviewed</i></td>
  </tr>
  <tr>
    <td><code>starred</code><br /><small>Boolean</small></td>
    <td><code>is:starred</code><br /><i>Returns all starred items</i></td>
  </tr>
  <tr>
    <td><code>title</code><br /><small>Text</small></td>
    <td><code>title:"keyboard"</code><br /><i>Returns all items where the title contains the word "keyboard"</i></td>
  </tr>
  <tr>
    <td><code>unread</code><br /><small>Boolean</small></td>
    <td><code>is:unread</code><br /><i>Returns all unread items</i></td>
  </tr>
  <tr>
    <td><code>vulnerability</code><br /><small>Boolean</small></td>
    <td><code>is:vulnerability</code><br /><i>Returns all vulnerability</i></td>
  </tr>
</tbody>
</table>

# Special cases
<p>To make queries easier to read there are some special keywords in GQL which makes queries easier to read in some places. Using these keywords are completely optional and might be used in cases where the query doesn't quite "read right".</p>
<p>You can use the <code>is:</code> prefix for boolean values. Normally <code>:</code> means contains, but in this case it will evaluate to true if the provided boolean is true.</p>

```custom
is:pullrequest or is:vulnerability
```

<p>translates to</p>

```custom
pullrequest or vulnerability
```