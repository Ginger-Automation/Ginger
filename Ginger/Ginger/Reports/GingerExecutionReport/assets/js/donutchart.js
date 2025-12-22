function CreateChart(inputDatax, id, ChartTitle, Total) {
    const inputData = [];
    var j = 0;
    for (var i = 0; i < inputDatax.length; i++) {
        if (inputDatax[i].value != 0) {
            inputData[j] = inputDatax[i];
            j = j + 1;
        }
    }
    const { PieChart, Pie, Legend, Cell, XAxis, YAxis, CartesianGrid, Tooltip } = Recharts;
    const CustomLabel = React.createClass({
        propTypes: {
            type: PropTypes.string,
            payload: PropTypes.array,
            label: PropTypes.string,
        },
        render() {
            const RADIAN = Math.PI / 180;
            const { cx, cy, midAngle, innerRadius, outerRadius, startAngle, endAngle,
                fill, payload, percent, value, active, label, name, color } = this.props;

            const sin = Math.sin(-RADIAN * midAngle);
            const cos = Math.cos(-RADIAN * midAngle);
            const sx = cx + (outerRadius + 5) * cos;
            const sy = cy + (outerRadius + 5) * sin;
            const mx = cx + (outerRadius + 20) * cos;
            const my = cy + (outerRadius + 20) * sin;
            const ex = mx + (cos >= 0 ? 1 : -1) * 20;
            const ey = my;
            const textAnchor = cos >= 0 ? 'start' : 'end';

            return (
                <g>
                    <path d={`M${sx},${sy}L${mx},${my}L${ex},${ey}`} stroke={fill} fill="none" />
                    <circle cx={ex} cy={ey} r={2} fill={fill} stroke="none" />
                    <text x={ex + (cos >= 0 ? 1 : -1) * 12} y={ey} textAnchor={textAnchor} size="12" fontSize="12" fill={fill}>
                        {`${name}(${value})`}
                    </text>
                </g>
            );
        }


    });

    function Getcolor(props) {
        if (props.name == "Passed")
            return '#54A81B';
        else if (props.name == "Failed")
            return '#E31123';
        else if (props.name == "Stopped")
            return '#ED5588';
        else if (props.name == "Other")
            return '#1B3651';
        else if (props.name == "NA")
            return '#e3dfdb';
    }

    const DonutChart = React.createClass({
        render() {
            const { value, name } = this.props;
            return (
                <PieChart width={400} height={250}>
                    <text x={185} y={40} textAnchor="middle" dominantBaseline="middle" fill="#1B3651" fontSize="16" fontWeight="Bold">{ChartTitle}</text>
                    <circle cx={185} cy={155} r={30} fill="#e3dfdb" stroke="none" />
                    <text x={185} y={155} textAnchor="middle" dominantBaseline="middle" fill="#1B3651" fontWeight="Bold">{Total}</text>
                    <Pie data={inputData} nameKey="name" dataKey="value" cx={180} cy={150} innerRadius={45} outerRadius={55} fill="#82ca9d"
                        label={<CustomLabel />}>
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