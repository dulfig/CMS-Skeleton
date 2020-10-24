(function () {
    'use strict';
    $(initSideBar);
    function initSideBar() {
        $(document).ready(function () {
            $('#sidebarCollapse').on('click', function () {
                $('#sidebar').toggleClass('active');
            });
        });
    }
})();