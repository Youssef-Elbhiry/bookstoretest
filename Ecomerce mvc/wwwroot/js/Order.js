

document.addEventListener("DOMContentLoaded", function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        LoadDataTable("inprocess");
    }
    else if (url.includes("pending")) {
        LoadDataTable("pending");
    }
    else if (url.includes("approved")) {
        LoadDataTable("approved");
    }
    else if (url.includes("completed")) {
        LoadDataTable("completed");
    }
    else {
        LoadDataTable();
    }
});

function LoadDataTable(status) {
    $('#myTable').DataTable({
        ajax: "/Admin/Order/GetAll?status=" + status,
        "columns": [
            {data : 'id' , "width" : "5%"},
            {data : 'name' , "width" : "15%"},
            { data: 'user.email' , "width" : "20%"},
            {data : 'phoneNumber' , "width" : "15%"},
            { data: 'orderStatus', "width": "15%" },
            { data: 'orderTotal', "width": "10%" },
            { data: 'id',
                render: function (data) {
                    return `<div>
                    <a href="/Admin/Order/Detail/${data}" class="btn btn-primary mx-2">Detail</a>
                    </div>`
                }
            , "width": "15%" }
        ]
    })
}