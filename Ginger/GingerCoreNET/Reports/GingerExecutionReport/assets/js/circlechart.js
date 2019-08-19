function CreateChart(inputData, id, ChartTitle, Total, Percent) {

    const { PieChart, Pie, Legend, Sector, Cell } = Recharts;
    function Getcolor(props) {
        if (props.name == "Passed")
            return '#54A81B';
        else if (props.name == "Failed")
            return '#E31123';
        else if (props.name == "Stopped")
            return '#ED5588';
        else if (props.name == "Other")
            return '#A21025';
        else if (props.name == "NA")
            return '#e3dfdb';
    }
    if (Total == 0) {
        inputData = [{ name: 'NA', value: 100 }];
    }
    const DonutChart = React.createClass({
        render() {
            return (
                <PieChart width={140} height={140}>
                    <circle cx={70} cy={55} r={30} fill="#e3dfdb" stroke="none" />
                    <text x={70} y={55} textAnchor="middle" dominantBaseline="middle" fill="#1B3651" fontWeight="Bold">{Percent}%</text>
                    <text x={70} y={130} textAnchor="middle" dominantBaseline="middle" fill="#1B3651" fontSize="15" fontWeight="Bold">{ChartTitle}({Total})</text>
                    <Pie data={inputData}
                        cx={65}
                        cy={50}
                        labelLine={false}
                        innerRadius={40}
                        outerRadius={50}
                        fill="#e3dfdb"
                        textAnchor='end'>
                        {
                            inputData.map((entry, index) => <Cell fill={Getcolor(inputData[index])} />)
                        }
                    </Pie>
                </PieChart>
            );
        }
    })

    ReactDOM.render(
        <DonutChart />,
        document.getElementById(id)
    );

}