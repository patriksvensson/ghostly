Title: Operators
Order: 2
Description: Explains the different operators available when querying in Ghostly
---

### Comparison operators

<div class="table-responsive">
<table class="table">
  <tbody>
  <tr>
    <th>Operator</th>
    <th>Example</th>
  </tr>
  <tr>
    <td><code>=</code></td>
    <td>
        <code>author=patriksvensson</code> matches all items
        where the author is exactly <code>patriksvensson</code>.
    </td>
  </tr>
  <tr>
    <td><code>:</code></td>
    <td>
        <code>author:patrik</code> matches all items
        where the author contains <code>patrik</code> such as 
        <code>patriksvensson</code>.
    </td>
  </tr>
  <tr>
    <td><code>!=</code></td>
    <td>
        <code>author!=patriksvensson</code> matches all items
        where the author is not <code>patriksvensson</code>.
    </td>
  </tr>
  <tr>
    <td><code>&lt;</code></td>
    <td>
        <code>id&lt;100</code> matches all items
        where the GitHub ID is less than 100.
    </td>
  </tr>
  <tr>
    <td><code>&lt;=</code></td>
    <td>
        <code>id&lt;=100</code> matches all items
        where the GitHub ID is less than or equal to <code>100</code>.
    </td>
  </tr>
  <tr>
    <td><code>&gt;</code></td>
    <td>
        <code>id&gt;100</code> matches all items
        where the GitHub ID is greater than <code>100</code>.
    </td>
  </tr>
  <tr>
    <td><code>&lt;=</code></td>
    <td>
        <code>id&gt;=100</code> matches all items
        where the GitHub ID is greater than or equal to <code>100</code>.
    </td>
  </tr>
</tbody>
</table>
</div>

### Logical operators

#### And

If you need to combine two or more statements via a conjunction you can use the `AND` logical operator.

```custom
author=patriksvensson and id>100
```

Will return all items which have `patriksvensson` as an author and
an ID that it less than `100`

#### Or

If you need to combine two or more statements via a disjunction you can use the `OR` logical operator.

```custom
author=patriksvensson or author=gep13
```

Will return all items which either have `patriksvensson` or `gep13` as an author.

#### Not

Sometimes you want to exclude a specific result from a query. You can do this by using the `NOT` keyword.

```custom
not author:patrik
```

Will return all items where the author's username does not contain `patrik`.