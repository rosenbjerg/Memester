import { FunctionalComponent, h } from "preact";
import { Link } from "preact-router/match";
import * as style from "./style.css";
//import Login from "../login";


const Header: FunctionalComponent = () => {



    return (
        <header class={style.header}>
            <h1>MEMESTER</h1>
            <nav>
                <Link activeClassName={style.active} href="/login">
                    login
                    {/*<Login/>*/}
                </Link>
            </nav>
        </header>
    );
};

export default Header;
