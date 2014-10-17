# sfcookbook-cookbook

Cookbook for SoundFingerprinting project.
## Supported Platforms

TODO: List your supported platforms.

## Attributes

<table>
  <tr>
    <th>Key</th>
    <th>Type</th>
    <th>Description</th>
    <th>Default</th>
  </tr>
  <tr>
    <td><tt>['sfcookbook']['bacon']</tt></td>
    <td>Boolean</td>
    <td>whether to include bacon</td>
    <td><tt>true</tt></td>
  </tr>
</table>

## Usage

### sfcookbook::default

Include `sfcookbook` in your node's `run_list`:

```json
{
  "run_list": [
    "recipe[sfcookbook::default]"
  ]
}
```

## License and Authors

Author:: Ciumac Sergiu (ciumac.sergiu@gmail.com)
