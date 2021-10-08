/*
 * Title: Copy DAX Comment into the measure's description field.
 * 
 * Author: Dan Meissner, twitter.com/danmeissner 
 * 
 * This script, when executed, will loop through all the measures in the model and
 * look for the first comment (aka. green text :) ) as designated by "//". It will copy the entire comment
 * until the next line break and put that into the measure's description.
 * 
 * If no comment of this type exists within the DAX Expression, then it copies the entire 
 * DAX expression into the measure's description.
 * 
 * At this point it only works on the first comment and for comments using slashes "//" but 
 * hope someone (or myself) can improve it to collect all comments with slashes 
 * or slash star formats.
 */

foreach(var m in Model.AllMeasures) {
  // Find the first comment using the two forward slashes
      string DAXCode = m.Expression;
      string separator1 = "//";
      string result1 = "";

      // Part A: get index of separator.
      int separatorIndex1 = DAXCode.IndexOf(separator1);

      // Part B: if separator exists, get substring.
      if (separatorIndex1 >= 0) {
        result1 = DAXCode.Substring(separatorIndex1 + separator1.Length);
      }
  // Repeat to find the first line break after the first comment.
      string separator2 = "\n";
      string result2 = "";

      // Part A: get index of separator.
      int separatorIndex2 = result1.IndexOf(separator2);

      // Part B: if separator exists, get substring.
      if (separatorIndex2 >= 0) {
        result2 = result1.Substring(0,separatorIndex2);
      }
  // If there is a comment, then add it to the description, 
  // otherwise add the entire expression
  if (m.Expression.Contains("//")) {
    m.Description = result2;
  } else {
    m.Description = m.Expression;
  };
}