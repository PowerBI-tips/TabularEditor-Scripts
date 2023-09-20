/*
 * Title: Format All Measures
 *
 * Authors: Matt Allington, https://exceleratorbi.com.au
 *          trjohnson19,    https://github.com/trjohnson19
 *
 * This script loops through the measures in the model and queries daxformatter.com
 * to format the measures in a batch process.
 *
 * This script uses the `FormatDax` overloaded extension method introduced in TE2 2.13.0.
 * FormatDax deprecation notice: https://docs.tabulareditor.com/te2/FormatDax.html.
 */

/* Format either all measures or selected measures. */
var _measures = Model.AllMeasures;
//var _measures = Selected.Measures;

/* Format all measures using default long lines and with a space after the function name. */
_measures.FormatDax();

/*
 * Alternative method to customize formatting returned.
 * `FormatDax(this IEnumerable<IDaxDependantObject> objects, bool shortFormat = false, bool? skipSpaceAfterFunctionName = null)`
 */
//_measures.FormatDax(false, null);

/*
 * Ensure measure text has a leading newline.
 * Fixes issue of measure expression starting directly after `<measure name> = ` on line 1.
 */
foreach (var _m in _measures) {
    _m.Expression = "\n" + _m.Expression
}

/*
 * Deprecated `FormatDax` method for use in TE2 < 2.13.
 *
 * foreach (var _m in _measures) {
 *     _m.Expression = FormatDax(_m.Expression);
 * }
 */
