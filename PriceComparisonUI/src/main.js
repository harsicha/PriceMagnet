import React, { useState, useEffect } from 'react';
import './main.css'
import { COMPANY_NAME, DESCRIPTION, Sources, SEARCH_PLACEHOLDER, content } from './constants.js';
import arrowicon from './icons/icons8-arrow-24.png'
import axios from 'axios';

export function Header() {
    return (
        <div class='header'>
            <h1 class='comp-h1'>{COMPANY_NAME}</h1>
            <p class='desc-p' align="center">{DESCRIPTION}</p>
        </div>
    );
}

export function Main() {
    const [sourceFilter, SetSourceFilter] = useState([]);
    const [sources, setSources] = useState([])
    const [selected, setSelected] = useState([]);
    const [data, setData] = useState([]); // use this to store data recieved from API
    const [depts, setDepts] = useState({});
    const [isLoading, setIsLoading] = useState(false);

    function HandleChange(e) {
        let source = e.target.id;
        console.log(source);
        if (!selected.includes(source)) {
            setSelected(prevState => {
                return prevState.concat(source);
            });
        }
        else {
            let filteredState = selected.filter(s => s !== source);
            setSelected(filteredState);
        }

    }

    function PrintVendors() {
        content.forEach(x => console.log(x.vendorName))
    }

    function GetData(e, query) {
        //let data = content;
        //setData(content);
        setIsLoading(true);
        e.preventDefault();
        axios.get('https://localhost:7234/api/ConsolidatedPrice/Get/' + query)
            .then(response => {
                console.log(response);
                setData([]);
                response.data.forEach(x => {
                    setData(prevState => {
                        return Normalize(prevState, x.commons);
                    });
                    setDepts(prevState => {
                        return x.commons.length == 0 ? prevState : {
                            ...prevState,
                            [x.commons[0].vendorName]: x.departments
                        };
                    });
                });
            })
            .catch(error => {
                console.log(error);
            })
            .finally(() => {
                setIsLoading(false);
            });
        // content.forEach(x => {
        //     setData(prevState => {
        //         return prevState.concat(x.commons);
        //     });
        //     setDepts(prevState => {
        //         return {
        //             ...prevState,
        //             [x.commons[0].vendorName]: x.departments
        //         }
        //     });
        // });
        console.log(query);
    }

    function Normalize(a, b) {
        console.log(`a: ${a.length}, b: ${b.length}`)
        if (a.length == 0) return b;
        if (b.length == 0) return a;
        let c = [];
        let lower;
        let higher;
        if (a.length > b.length) {
            higher = a;
            lower = b;
        } else {
            higher = b;
            lower = a
        }
        let inner = Math.ceil((1.0 * higher.length) / (1.0 * lower.length));
        let ind = 0, j = 0;
        for (let i = 0; i < lower.length; i++) {
            if (lower[i] !== undefined) {
                c.push(lower[i]);
            }
            for (j = ind; j < (inner + ind); j++) {
                try {
                    if (higher[j] !== undefined) {
                        c.push(higher[j]);
                    }
                }
                catch (error) {
                    console.log('Exceeded!');
                }
            }
            ind = j;
        }
        return c;
    }

    useEffect(() => {
        console.log('Selected Array: ' + selected);
        data.forEach(x => {
            setSources(prevState => {
                return prevState.includes(x.vendorName) ? prevState : prevState.concat(x.vendorName);
            });
        });
        for (let dept in depts) {
            console.log(dept + ': ' + depts[dept]);
        }
    })

    return (
        <div>
            <Header></Header>
            <Filters sources={sources} sourceFilter={sourceFilter} handleChange={HandleChange}></Filters>
            <Search data={data} getData={GetData} selected={selected} isLoading={isLoading} depts={depts}></Search>
        </div>
    )
}

class Search extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            isRelevanceSelected: true,
            isNameSelected: false,
            isPriceSelected: false,
            isNameASC: false,
            isPriceASC: false,
            isNameArrowVisible: false,
            isPriceArrowVisible: false,
            query: '',
        };

        this.Result = this.Result.bind(this);
        this.handleBtnChange = this.handleBtnChange.bind(this);
        this.SearchBar = this.SearchBar.bind(this);
        this.RenderCards = this.RenderCards.bind(this);
    }

    // componentDidUpdate() {
    //     console.log(this.state.query);
    // }

    SearchBar() {
        return (
            <div class='search'>
                <input type='text' placeholder={SEARCH_PLACEHOLDER} id='searchBar' onKeyDown={e => {
                    if (e.key === 'Enter' && !this.props.isLoading) {
                        console.log('Hit the API!'); // After getting data from API, update the sources array in the same method.
                        this.props.getData(e, this.state.query);
                    }
                }} onChange={e => {
                    this.setState({
                        query: e.target.value
                    });
                }}
                    disabled={this.props.isLoading}
                ></input>
                <button onClick={e => {
                    console.log('Hit the API!');
                    this.props.getData(e, this.state.query);
                }}
                    class='searchButton'
                    disabled={this.props.isLoading}
                >Search!</button>
            </div>
        )
    }

    Result() {
        return (
            <div class='sort'>
                <span id='sortby'>SORT BY</span>
                <span id='rel'>
                    <button
                        id='relbtn'
                        onClick={this.handleBtnChange}
                        class={
                            this.state.isRelevanceSelected ? 'btnSelected' : 'btnNotSelected'
                        }
                    >
                        RELEVANCE</button>
                </span>
                <span id='name'>
                    <button
                        id='namebtn'
                        onClick={this.handleBtnChange}
                        class={
                            this.state.isNameSelected ? 'btnSelected' : 'btnNotSelected'
                        }
                    >
                        NAME</button>
                </span>
                <img src={arrowicon}
                    id='namearr'
                    class={this.state.isNameSelected ? this.state.isNameASC ? 'downArrow unhide' : 'upArrow unhide' : 'hide'}
                />
                <span id='price'>
                    <button
                        id='pricebtn'
                        onClick={this.handleBtnChange}
                        class={
                            this.state.isPriceSelected ? 'btnSelected' : 'btnNotSelected'
                        }
                    >
                        PRICE</button>
                </span>
                <img src={arrowicon}
                    id='pricearr'
                    class={this.state.isPriceSelected ? this.state.isPriceASC ? 'downArrow unhide' : 'upArrow unhide' : 'hide'}
                />
            </div>
        )
    }

    RenderCards() {
        console.log('Inside RenderCards!');
        let data = structuredClone(this.props.data);
        if (data.length == 0) {
            return (
                <div class='noDataDiv'>
                    <p><i>Use the search bar to search for products!</i></p>
                </div>
            )
        }
        let resultJSX = [];

        if (this.props.selected.length !== 0) {
            data = data.filter(x => {
                return this.props.selected.includes(x.vendorName)
            })
        }
        if (this.state.isNameSelected) {
            if (this.state.isNameASC) {
                data = data.sort((a, b) => ('' + a.productName).localeCompare(b.productName))
            }
            else {
                data = data.sort((a, b) => ('' + b.productName).localeCompare(a.productName))
            }
        }
        else if (this.state.isPriceSelected) {
            if (this.state.isPriceASC) {
                data = data.sort((a, b) => (parseInt(a.productPrice.price.replaceAll(',', '')) > parseInt(b.productPrice.price.replaceAll(',', ''))) ? 1 : (parseInt(a.productPrice.price.replaceAll(',', '')) < parseInt(b.productPrice.price.replaceAll(',', ''))) ? -1 : 0)
            }
            else {
                data = data.sort((a, b) => (parseInt(a.productPrice.price.replaceAll(',', '')) > parseInt(b.productPrice.price.replaceAll(',', ''))) ? -1 : (parseInt(a.productPrice.price.replaceAll(',', '')) < parseInt(b.productPrice.price.replaceAll(',', ''))) ? 1 : 0)
            }
        }
        // if (this.props.selected.length == 0) {
        //     data = data;
        // }
        // else {
        //     data = data.filter(x => {
        //         x.commons.forEach(y => {
        //             this.props.selected.includes(y.vendorName)
        //         })
        //     });
        //     for (let i = 0; i < data.length; i++) {
        //         data[i].commons = data[i].commons.filter(x => {
        //             return this.props.selected.includes(x.vendorName)
        //         })
        //     }
        // }
        // if (this.state.isRelevanceSelected) {
        //     data = data;
        // }
        // if (this.state.isNameSelected) {
        //     for (let i = 0; i < data.length; i++) {
        //         if (this.state.isNameASC) {
        //             data[i].commons = data[i].commons.sort((a, b) => (a.productName > b.productName) ? 1 : (a.productName < b.productName) ? -1 : 0)
        //         }
        //         else {
        //             data[i].commons = data[i].commons.sort((a, b) => (a.productName > b.productName) ? -1 : (a.productName < b.productName) ? 1 : 0)
        //         }
        //     }
        //     // if (this.state.isNameASC) data = data.sort(x => {
        //     //     return x.commons.forEach((a, b) => (a.productName > b.productName) ? 1 : (a.productName < b.productName) ? -1 : 0);
        //     // });
        //     // else data = data.sort(x => {
        //     //     return x.commons.forEach((a, b) => (a.productName > b.productName) ? -1 : (a.productName < b.productName) ? 1 : 0);
        //     // });
        // }
        // else if (this.state.isPriceSelected) {
        //     if (this.state.isPriceASC) data = data.sort(x => {
        //         return x.commons.forEach((a, b) => (parseInt(a.productPrice.price.replace(',', '')) > parseInt(b.productPrice.price.replace(',', ''))) ? 1 : (parseInt(a.productPrice.price.replace(',', '')) < parseInt(b.productPrice.price.replace(',', ''))) ? -1 : 0);
        //     });
        //     else data = data.sort(x => {
        //         return x.commons.forEach((a, b) => (parseInt(a.productPrice.price.replace(',', '')) > parseInt(b.productPrice.price.replace(',', ''))) ? -1 : (parseInt(a.productPrice.price.replace(',', '')) < parseInt(b.productPrice.price.replace(',', ''))) ? 1 : 0);
        //     })
        // }
        let cnt = 0;
        console.log(data);
        data.forEach(x => {
            resultJSX.push(
                <div key={cnt++} class='card'>
                    <div class='imgDiv'><a class='cardAnchor' href={x.productLink} target='_blank'><img src={x.productImageLink} class='cardIMG'></img></a></div>
                    <div class='infoDiv'>
                        <div class='bubbleDiv'>
                            {/* <span class='bubble'>Free Delivery</span>
                            <span class='bubble'>Type C</span>
                            <span class='bubble'>Cable</span>
                            <span class='bubble'>Mobiles & Accesories</span>
                            <span class='bubble'>Computers & Accessories</span> */}
                            {
                                Object.keys(this.props.depts).filter(y => y === x.vendorName).map((prop, key) => {
                                    let deptJSX = [];
                                    let depts = this.props.depts[prop];
                                    for (let dept in depts) {
                                        deptJSX.push(<span class='bubble' key={key + dept}>{depts[dept]}</span>);
                                    }
                                    return deptJSX;
                                })
                            }
                        </div>
                        <div class='nameDiv'><a class='cardAnchor' href={x.productLink} target='_blank'><span class='name'>{x.productName}</span></a></div>
                        {/* <div class='moreinfo'>
                            <span class='short'>A feature</span>
                            <span class='short'>Some feature</span>
                            <span class='short'>Some other feature</span>
                            <span class='short'>Yet another feature</span>
                        </div> */}
                        <div class='moreinfo'>
                            <ul>
                                {
                                    x.deliveryInformation.type.length > 0 ? x.deliveryInformation.getItBy.length > 0 ? (<li><span class='short' key={'Delivery' + cnt}>{x.deliveryInformation.type + ': ' + x.deliveryInformation.getItBy}</span></li>) : (<li><span class='short' key={'Delivery' + cnt}>{x.deliveryInformation.type}</span></li>) : null
                                }
                                {
                                    x.moreInfo.map((info) => {
                                        return <li class='short' key={info + cnt}>{info}</li>;
                                    })
                                }
                            </ul>
                        </div>
                    </div>
                    <div class='priceDiv'>
                        <span class={x.vendorName.toLowerCase()}>{x.vendorName}</span>
                        <span class='priceSP'>{x.productPrice.symbol + x.productPrice.price}</span>
                        <span class='reviewsSP'>{x.reviews.count}</span>
                    </div>
                </div>
            );
        });
        return resultJSX;
    }

    handleBtnChange(e) {
        console.log('Inside handleBtnChange!');
        console.log(e.target.id);
        if (e.target.id === 'relbtn') {
            this.setState(
                {
                    isRelevanceSelected: true,
                    isNameSelected: false,
                    isPriceSelected: false,
                    isNameASC: false,
                    isPriceASC: false,
                }
            )
        }
        else if (e.target.id === 'namebtn') {
            this.setState(
                {
                    isRelevanceSelected: false,
                    isNameSelected: true,
                    isPriceSelected: false,
                    isNameASC: !this.state.isNameASC,
                    isPriceASC: false,
                }
            );
        }
        else if (e.target.id === 'pricebtn') {
            this.setState(
                {
                    isRelevanceSelected: false,
                    isNameSelected: false,
                    isPriceSelected: true,
                    isNameASC: false,
                    isPriceASC: !this.state.isPriceASC,
                }
            );
        }
    }

    render() {
        return (
            <div class='sr'>
                <this.SearchBar></this.SearchBar>

                <div class='result'>
                    <this.Result></this.Result>
                    <div class='cardHolder'>
                        <this.RenderCards></this.RenderCards>
                    </div>
                </div>

            </div>
        )
    }
}

export class Filters extends React.Component {
    constructor(props) {
        super(props);
        this.SourceFilterTemp = this.SourceFilterTemp.bind(this);
    }

    SourceFilterTemp() {
        let filter = [];
        let sources = this.props.sources;
        let count = 0;
        for (let key in sources) {
            // console.log(key + "; " + sources[key]);
            let source = sources[key]
            filter.push(<div class={"pretty p-default p-curve p-smooth" + source} key={source}>
                <input type="checkbox" name={source} onChange={(e) => this.props.handleChange(e)} id={source} />
                <div class="state">
                    <label>{source}</label>
                </div>
            </div>);
        }
        // console.log(filter);
        return filter;
    }

    render() {
        return (
            <div class='filters'>
                <div id='filterHeader'><span>FILTERS</span></div>
                <div class='hrcont'><hr></hr></div>
                <div id='source' multiple><this.SourceFilterTemp></this.SourceFilterTemp></div>
            </div>
        )
    }
} 