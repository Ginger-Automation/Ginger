// Ginger will replace with the code to inject

// Enable to Inject/Add Java script to page

// Ginger will replace with the code to inject

var s = "%SCRIPT%" 

// Something to show that we injected the script, will highligh then go down 
var el = document.createElement('div'),
b = document.getElementsByTagName('body')[0];
otherlib = false,
msg = '';
el.style.position = 'fixed';
el.style.height = '32px';
el.style.width = '220px';
el.style.marginLeft = '-110px';
el.style.top = '0';
el.style.left = '50%';
el.style.padding = '5px 10px';
el.style.zIndex = 1001;
el.style.fontSize = '12px';
el.style.color = '#222';
el.style.backgroundColor = '#f99';


AddScript(s);   

function AddScript(JavaScriptCode){
var script=document.createElement('script');
// script.src = "https://ajax.googleapis.com/ajax/libs/jquery/2.2.0/jquery.min.js";  // In case we want ref to url and not source code
script.type = "text/javascript";
script.text = JavaScriptCode;
var head=document.getElementsByTagName('head')[0],
done = false;

// Attach handlers for all browsers
script.onload=script.onreadystatechange = function(){
    if ( !done && (!this.readyState
      || this.readyState == 'loaded'
      || this.readyState == 'complete') ) {
        done=true;

        //remove the script if needed
        // success();
        // script.onload = script.onreadystatechange = null;
        // head.removeChild(script);
    }
};
head.appendChild(script);

showMsg("Ginger Script Injected!");
}

//getScript('http://code.jquery.com/jquery-latest.min.js',function() {
//    if (typeof jQuery=='undefined') {
//        msg='Sorry, but jQuery wasn\'t able to load';
//    } else {
//        msg='This page is now jQuerified with v' + jQuery.fn.jquery;
//        if (otherlib) {msg+=' and noConflict(). Use $jq(), not $().';}
//    }
//    return showMsg();
//});
function showMsg(msg) {
    el.innerHTML=msg;
    b.appendChild(el);
    window.setTimeout(function() {
        if (typeof jQuery=='undefined') {
            b.removeChild(el);
        } else {
            jQuery(el).fadeOut('slow',function() {
                jQuery(this).remove();
            });
            if (otherlib) {
                $jq=jQuery.noConflict();
            }
        }
    } ,2500);
}

